using Gifytools.Database.Entities;
using Gifytools.Settings;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;

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

		var fullOutputPath = Path.Combine(Directory.GetCurrentDirectory(), _settings.GifOutputPath,
			$"{Path.GetFileNameWithoutExtension(entity.VideoInputPath)}_{DateTime.UtcNow:yyyyMMdd_HHmmss}.gif");

		List<string> ffmpegArgs = FillFfmpegArgs(entity);

		// Build the complete filter chain
		List<string> filterList = BuildFilterChain(entity);

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

		await RunFFmpegCommandAsync(ffmpegArgs, 60000);
		return fullOutputPath;
	}

	private List<string> FillFfmpegArgs(ConversionRequestEntity entity)
	{
		List<string> ffmpegArgs = new List<string>();
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


		return ffmpegArgs;
	}

	private List<string> BuildFilterChain(ConversionRequestEntity entity)
	{
		List<string> filterList = new List<string>();

		if (entity.SetCrop)
		{
			filterList.Add($"crop={entity.CropWidth}:{entity.CropHeight}:{entity.CropX}:{entity.CropY}");
		}

		filterList.Add($"scale={entity.Width}:-1:flags=lanczos");

		if (entity.SetReduceFrames)
		{
			filterList.Add($"fps={entity.FrameSkipInterval}");
		}

		if (entity.SetSpeed && entity.SpeedMultiplier > 0)
		{
			double speedFactor = 1 / entity.SpeedMultiplier;
			filterList.Add($"setpts={speedFactor}*PTS");
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

		return filterList;
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
	private async Task RunFFmpegCommandAsync(List<string> arguments, int timeoutMs = 30000, CancellationToken cancellationToken = default)
	{
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
				throw new TimeoutException($"FFmpeg process timed out after {timeoutMs} ms.");
			}
		}
		catch (OperationCanceledException) when (timeoutCts.Token.IsCancellationRequested)
		{
			process.Kill(true);
			throw new TimeoutException($"FFmpeg process timed out after {timeoutMs} ms.");
		}
		catch (OperationCanceledException)
		{
			process.Kill(true);
			throw;
		}

		string output = outputBuilder.ToString();
		string error = errorBuilder.ToString();

		if (process.ExitCode != 0)
		{
			throw new InvalidOperationException($"FFmpeg command failed.\\nError: {error}\\nOutput: {output}");
		}
	}
}