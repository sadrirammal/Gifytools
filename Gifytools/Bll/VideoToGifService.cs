using Gifytools.Database.Entities;
using Gifytools.Diagnostics;
using Gifytools.Settings;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

namespace Gifytools.Bll;

public class VideoToGifService : IVideoToGifService
{
    private readonly FileSystemSettings _settings;
    private readonly GifyMetrics _metrics;
    private readonly ILogger<VideoToGifService> _logger;

    // Constructor to set the path to ffmpeg
    public VideoToGifService(IOptions<FileSystemSettings> settings, GifyMetrics metrics, ILogger<VideoToGifService> logger)
    {
        _settings = settings.Value;
        if (string.IsNullOrEmpty(_settings.FFmpegPath) || !File.Exists(_settings.FFmpegPath))
        {
            throw new ArgumentException("FFmpeg executable not found at the specified path.", nameof(_settings.FFmpegPath));
        }

        //Create directories if they dont exist
        if (!Directory.Exists(_settings.VideoInputPath))
        {
            logger.LogInformation("{inputPathName} {inputPath} does not exist, creating it.", nameof(_settings.VideoInputPath), _settings.VideoInputPath);
            Directory.CreateDirectory(_settings.VideoInputPath);
        }

        if (!Directory.Exists(_settings.GifOutputPath))
        {
            logger.LogInformation("{outputPathName} {outputPath} does not exist, creating it.", nameof(_settings.GifOutputPath), _settings.GifOutputPath);
            Directory.CreateDirectory(_settings.GifOutputPath);
        }

        _metrics = metrics;
        _logger = logger;
    }

    public async Task<string> ConvertToGif(ConversionRequestEntity entity)
    {
        var activity = Activity.Current;
        if (string.IsNullOrEmpty(entity.VideoInputPath) || !File.Exists(entity.VideoInputPath))
        {
            throw new ArgumentException("Input video file does not exist.", nameof(entity.VideoInputPath));
        }
        if (string.IsNullOrEmpty(_settings.GifOutputPath))
        {
            throw new ArgumentException("Output path is invalid.", nameof(_settings.GifOutputPath));
        }

        var fullOutputPath = Path.Combine(Directory.GetCurrentDirectory(), _settings.GifOutputPath,
            $"{Path.GetFileNameWithoutExtension(entity.VideoInputPath)}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.gif");

        var ffmpegArgs = new List<string>();
        ffmpegArgs.Add("-i");
        ffmpegArgs.Add(entity.VideoInputPath);

        if (entity.SetStartTime)
        {
            ffmpegArgs.Add("-ss");
            ffmpegArgs.Add(entity.StartTime?.ToString() ?? "0");
        }

        if (entity.SetEndTime)
        {
            ffmpegArgs.Add("-to");
            ffmpegArgs.Add(entity.EndTime?.ToString() ?? "0");
        }

        if (entity.SetFps)
        {
            ffmpegArgs.Add("-r");
            ffmpegArgs.Add(entity.Fps.ToString());
        }

        if (entity.SetSpeed && entity.SpeedMultiplier > 0)
        {
            double speedFactor = 1 / entity.SpeedMultiplier;
            ffmpegArgs.Add("-filter:v");
            ffmpegArgs.Add($"setpts={speedFactor}*PTS");
        }

        // Build the complete filter chain
        var filterList = new List<string>();

        if (entity.SetCrop)
        {
            filterList.Add($"crop={entity.CropWidth}:{entity.CropHeight}:{entity.CropX}:{entity.CropY}");
        }

        filterList.Add($"scale={entity.Width}:-1:flags=lanczos");

        if (entity.SetReduceFrames)
        {
            filterList.Add($"fps={entity.FrameSkipInterval}");
        }

        if (entity.SetReverse)
        {
            filterList.Add("reverse");
        }

        if (entity.SetWatermark && !string.IsNullOrEmpty(entity.WatermarkText))
        {
            string sanitizedText = Regex.Replace(entity.WatermarkText, "[^a-zA-Z0-9 .,!?@#%&*()_+=-]", "");
            var fontPath = _settings.Fonts.FirstOrDefault(x => x.Name == entity.WatermarkFont)?.Path ??
                          _settings.Fonts.First().Path;
            filterList.Add($"drawtext=text='{sanitizedText}':fontfile='{fontPath}':fontcolor=white:fontsize=24:x=10:y=10");
        }

        // Handle compression with palette generation using filter_complex
        if (entity.SetCompression)
        {
            // Use filter_complex for everything when compression is enabled
            string videoFilters = string.Join(",", filterList);
            string complexFilter = $"[0:v] {videoFilters} [v1]; [0:v] {videoFilters},palettegen=stats_mode=diff [p]; [v1][p] paletteuse=dither=none";

            ffmpegArgs.Add("-filter_complex");
            ffmpegArgs.Add(complexFilter);
        }
        else
        {
            // Use simple video filter when no compression
            if (filterList.Any())
            {
                ffmpegArgs.Add("-vf");
                ffmpegArgs.Add(string.Join(",", filterList));
            }
        }

        ffmpegArgs.Add("-c:v");
        ffmpegArgs.Add("gif");
        ffmpegArgs.Add(fullOutputPath);
        _logger.LogDebug("FFmpeg arguments: {args}", string.Join(' ', ffmpegArgs));

        await RunFFmpegCommandAsync(ffmpegArgs, 600000);
        return fullOutputPath;
    }


    public async Task<string> UploadVideo(IFormFile videoFile)
    {
        using var uploadActivity = GifyMetrics.GifytoolsActivitySource.StartActivity("Gifytools.UploadVideo");
        var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), _settings.VideoInputPath);
        var fileName = $"{Path.GetFileNameWithoutExtension(videoFile.FileName)}_{DateTime.UtcNow:yyyyMMdd_HHmmss}{Path.GetExtension(videoFile.FileName)}";
        var fullPath = Path.Combine(uploadFolder, fileName);
        uploadActivity?.SetCustomProperty(nameof(fullPath), fullPath);
        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await videoFile.CopyToAsync(stream);
        }

        _metrics.VideoUploaded(new FileInfo(fullPath).Length);
        uploadActivity?.SetStatus(ActivityStatusCode.Ok);
        return fullPath;
    }

    /// <summary>
    /// This function calls the the FFmpeg process and runs it. 
    /// </summary>
    /// <param name="arguments"></param>
    /// <param name="timeoutMs"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    /// <exception cref="TimeoutException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    private async Task RunFFmpegCommandAsync(List<string> arguments, int timeoutMs = 30000, CancellationToken cancellationToken = default)
    {
        using var ffmpegActivity = GifyMetrics.GifytoolsActivitySource.StartActivity("Gifytools.Ffmpeg", ActivityKind.Client);
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _settings.FFmpegPath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true
        };
        ffmpegActivity?.SetCustomProperty("path", _settings.FFmpegPath);
        ffmpegActivity?.SetCustomProperty("args", string.Join(' ', arguments));
        foreach (var arg in arguments)
        {
            process.StartInfo.ArgumentList.Add(arg);
        }

        var outputBuilder = new StringBuilder();
        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (sender, args) =>
        {
            if (args.Data != null)
                outputBuilder.AppendLine(args.Data);
        };

        process.ErrorDataReceived += (sender, args) =>
        {
            if (args.Data != null)
                errorBuilder.AppendLine(args.Data);
        };

        var processCompletion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        process.Exited += (sender, args) =>
        {
            processCompletion.TrySetResult(true);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        using var timeoutCts = new CancellationTokenSource(timeoutMs);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            await process.WaitForExitAsync(linkedCts.Token);

            if (!process.HasExited)
            {
                process.Kill(true);
                
                
                var ex = new TimeoutException($"FFmpeg process timed out after {timeoutMs} ms.");
                ffmpegActivity?.SetStatus(ActivityStatusCode.Error);
                ffmpegActivity?.AddException(ex);
                throw ex;
            }
        }
        catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
        {
            process.Kill(true);
            var ex = new TimeoutException($"FFmpeg process timed out after {timeoutMs} ms.");
            ffmpegActivity?.SetStatus(ActivityStatusCode.Error);
            ffmpegActivity?.AddException(ex);
            throw ex;
        }
        catch (OperationCanceledException ex)
        {
            process.Kill(true);
            ffmpegActivity?.SetStatus(ActivityStatusCode.Error);
            ffmpegActivity?.AddException(ex);
            throw;
        }

        string output = outputBuilder.ToString();
        string error = errorBuilder.ToString();

        if (process.ExitCode != 0)
        {
           var ex = new InvalidOperationException($"FFmpeg command failed.\\nError: {error}\\nOutput: {output}");
            ffmpegActivity?.SetStatus(ActivityStatusCode.Error);
            ffmpegActivity?.AddException(ex);
            throw ex;
        }
        ffmpegActivity?.SetStatus(ActivityStatusCode.Ok);
    }
}