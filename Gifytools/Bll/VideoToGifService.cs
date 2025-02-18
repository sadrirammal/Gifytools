using System.Diagnostics;
using System.Security.AccessControl;
using System.Text;
using Gifytools.Database.Entities;
using Gifytools.Settings;
using Microsoft.Extensions.Options;

namespace Gifytools.Bll;

public class VideoToGifService : IVideoToGifService
{
    private readonly FileSystemSettings _settings;

    // Constructor to set the path to ffmpeg
    public VideoToGifService(IOptions<FileSystemSettings> settings)
    {
        _settings = settings.Value;
        if (string.IsNullOrEmpty(_settings.FFmpegPath) || !File.Exists(_settings.FFmpegPath))
        {
            throw new ArgumentException("FFmpeg executable not found at the specified path.", nameof(_settings.FFmpegPath));
        }

        //Create directories if they dont exist
        if (!Directory.Exists(_settings.VideoInputPath))
        {
            Directory.CreateDirectory(_settings.VideoInputPath);
        }

        if (!Directory.Exists(_settings.GifOutputPath))
        {
            Directory.CreateDirectory(_settings.GifOutputPath);
        }
    }

    public async Task<string> ConvertToGif(ConversionRequestEntity entity)
    {
        if (string.IsNullOrEmpty(entity.VideoInputPath) || !File.Exists(entity.VideoInputPath))
        {
            throw new ArgumentException("Input video file does not exist.", nameof(entity.VideoInputPath));
        }

        if (string.IsNullOrEmpty(_settings.GifOutputPath))
        {
            throw new ArgumentException("Output path is invalid.", nameof(_settings.GifOutputPath));
        }

        var fullOutputPath = Path.Combine(Directory.GetCurrentDirectory(), _settings.GifOutputPath, $"{Path.GetFileNameWithoutExtension(entity.VideoInputPath)}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.gif");

        var ffmpegArgs = new List<string>();

        // Input File
        ffmpegArgs.Add($"-i \"{entity.VideoInputPath}\"");

        // Start & End Time (Trim)
        if (entity.SetStartTime)
        {
            ffmpegArgs.Add($"-ss {entity.StartTime}");
        }
        if (entity.SetEndTime)
        {
            ffmpegArgs.Add($"-to {entity.EndTime}");
        }

        // Frame Rate (Reduce FPS)
        if (entity.SetFps)
        {
            ffmpegArgs.Add($"-r {entity.Fps}");
        }

        // Speed Adjustment
        if (entity.SetSpeed && entity.SpeedMultiplier > 0)
        {
            double speedFactor = 1 / entity.SpeedMultiplier;
            ffmpegArgs.Add($"-filter:v \"setpts={speedFactor}*PTS\"");
        }

        // Build the single "-vf" filter chain
        var filterList = new List<string>();

        // Cropping
        if (entity.SetCrop)
        {
            filterList.Add($"crop={entity.CropWidth}:{entity.CropHeight}:{entity.CropX}:{entity.CropY}");
        }

        // Scaling (Width Only - Maintain Aspect Ratio)
        filterList.Add($"scale={entity.Width}:-1:flags=lanczos"); // -1 keeps aspect ratio

        // Reduce Frames (By skipping)
        if (entity.SetReduceFrames)
        {
            filterList.Add($"fps={entity.FrameSkipInterval}");
        }

        // Reverse GIF
        if (entity.SetReverse)
        {
            filterList.Add("reverse");
        }

        // Add Watermark
        if (entity.SetWatermark && !string.IsNullOrEmpty(entity.WatermarkText))
        {
            var fontPath = _settings.Fonts.Where(x => x.Name == entity.WatermarkFont).Select(x => x.Path).FirstOrDefault() ??
                           _settings.Fonts.Select(x => x.Path).First();

            filterList.Add($"drawtext=text='{entity.WatermarkText}':fontfile='{fontPath}':fontcolor=white:fontsize=24:x=10:y=10");
        }

        // Apply the single "-vf" argument only if filters exist
        if (filterList.Any())
        {
            ffmpegArgs.Add($"-vf \"{string.Join(",", filterList)}\"");
        }

        // Palette Optimization (Color Reduction for Quality)
        if (entity.SetCompression)
        {
            ffmpegArgs.Add("-filter_complex \"[0:v] palettegen=stats_mode=diff [p]; [0:v][p] paletteuse=dither=none\"");
        }

        // Output as GIF
        ffmpegArgs.Add($"-c:v gif \"{fullOutputPath}\"");

        string arguments = string.Join(" ", ffmpegArgs);

        await RunFFmpegCommandAsync(arguments, 600000);
        
        return fullOutputPath;
    }

    public async Task<string> UploadVideo(IFormFile videoFile)
    {
        var uploadFolder = Path.Combine(Directory.GetCurrentDirectory(), _settings.VideoInputPath);
        var fileName = $"{Path.GetFileNameWithoutExtension(videoFile.FileName)}_{DateTime.UtcNow:yyyyMMdd_HHmmss}{Path.GetExtension(videoFile.FileName)}";
        var fullPath = Path.Combine(uploadFolder, fileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await videoFile.CopyToAsync(stream);
        }

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
    private async Task RunFFmpegCommandAsync(string arguments, int timeoutMs = 30000, CancellationToken cancellationToken = default)
    {
        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _settings.FFmpegPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            },
            EnableRaisingEvents = true
        };

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
                process.Kill(true); // Ensure the process is terminated
                throw new TimeoutException($"FFmpeg process timed out after {timeoutMs} ms.");
            }
        }
        catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
        {
            process.Kill(true); // Kill process if timeout is reached
            throw new TimeoutException($"FFmpeg process timed out after {timeoutMs} ms.");
        }
        catch (OperationCanceledException)
        {
            process.Kill(true); // Kill process if user cancels operation
            throw;
        }
        catch (Exception ex)
        {
            throw ex;
        }

        string output = outputBuilder.ToString();
        string error = errorBuilder.ToString();

        Console.WriteLine($"FFmpeg Output: {output}");
        Console.WriteLine($"FFmpeg Error: {error}");

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"FFmpeg command failed.\nError: {error}\nOutput: {output}");
        }
    }
}