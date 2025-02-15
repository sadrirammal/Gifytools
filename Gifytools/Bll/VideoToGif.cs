using System.Diagnostics;

namespace Gifytools.Bll;

public class VideoToGif
{
    private readonly string _ffmpegPath;

    // Constructor to set the path to ffmpeg
    public VideoToGif(string ffmpegPath)
    {
        if (string.IsNullOrEmpty(ffmpegPath) || !File.Exists(ffmpegPath))
        {
            throw new ArgumentException("FFmpeg executable not found at the specified path.", nameof(ffmpegPath));
        }

        _ffmpegPath = ffmpegPath;
    }

    public string AddTextToGif(string inputGifPath, string outputGifPath, string text, int duration = 3, int width = 320, int height = 240, string fontPath = "C\\:/Windows/Fonts/arial.ttf", int fontSize = 24)
    {
        if (!File.Exists(inputGifPath))
        {
            throw new FileNotFoundException("Input GIF file not found.", inputGifPath);
        }

        string tempTextOverlayGif = Path.Combine(Path.GetTempPath(), "text_overlay.gif");
        string concatFilePath = Path.Combine(Path.GetTempPath(), "concat_list.txt");

        try
        {
            // Step 1: Create the text overlay GIF
            var overlayArgs = $"-f lavfi -i color=color=black:size={width}x{height}:duration={duration} " +
                              $"-vf \"drawtext=fontfile='{fontPath}':text='{text}':fontcolor=white:fontsize={fontSize}:x=(w-text_w)/2:y=(h-text_h)/2\" " +
                              $"-loop 0 {tempTextOverlayGif}";

            RunFFmpegCommand(overlayArgs);

            // Step 2: Create the concat file
            File.WriteAllText(concatFilePath, $"file '{tempTextOverlayGif}'\nfile '{inputGifPath}'\n");

            // Step 3: Concatenate the GIFs
            var concatArgs = $"-f concat -safe 0 -i {concatFilePath} -c copy {outputGifPath}";
            RunFFmpegCommand(concatArgs);

            return outputGifPath;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Failed to add text to the GIF.", ex);
        }
        finally
        {
            // Clean up temporary files
            if (File.Exists(tempTextOverlayGif)) File.Delete(tempTextOverlayGif);
            if (File.Exists(concatFilePath)) File.Delete(concatFilePath);
        }
    }

    private void RunFFmpegCommand(string arguments)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();

        string output = process.StandardOutput.ReadToEnd();
        string error = process.StandardError.ReadToEnd();

        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"FFmpeg command failed.\nError: {error}\nOutput: {output}");
        }
    }

    /// <summary>
    /// Converts a video file to a GIF.
    /// </summary>
    /// <param name="inputPath">Path to the input video file.</param>
    /// <param name="outputPath">Path to save the resulting GIF.</param>
    /// <param name="fps">Frames per second for the GIF (default is 10).</param>
    /// <param name="width">Width of the GIF (default is 720, aspect ratio preserved).</param>
    public void ConvertToGif(string inputPath, string outputPath, int fps = 15, int width = 720)
    {
        if (string.IsNullOrEmpty(inputPath) || !File.Exists(inputPath))
        {
            throw new ArgumentException("Input video file does not exist.", nameof(inputPath));
        }

        if (string.IsNullOrEmpty(outputPath))
        {
            throw new ArgumentException("Output path is invalid.", nameof(outputPath));
        }

        // Ensure output directory exists
        string outputDir = Path.GetDirectoryName(outputPath);
        if (!Directory.Exists(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        // FFmpeg command arguments
        string arguments = $"-i \"{inputPath}\" -vf \"fps={fps},scale={width}:-1:flags=lanczos\" -c:v gif \"{outputPath}\"";

        // Start FFmpeg process
        var processStartInfo = new ProcessStartInfo
        {
            FileName = _ffmpegPath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using (var process = new Process { StartInfo = processStartInfo })
        {
            process.Start();

            // Capture FFmpeg output for debugging
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();

            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new Exception($"FFmpeg process failed with error: {error}");
            }
        }
    }
}