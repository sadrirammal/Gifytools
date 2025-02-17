using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    private static readonly HttpClient client = new HttpClient();
    private static readonly Random random = new Random();

    static async Task Main(string[] args)
    {
        int requestCount = 10; // Adjust for load
        string apiUrl = "https://api.gifytools.com/api/gif/videoToGif";

        // List of video file paths
        List<string> videoFiles = new List<string>
        {
            "C:\\Data\\deckel.avi",
            "C:\\Data\\winkel.avi",
            "C:\\Data\\snowboard.avi",
            "C:\\Data\\tank.avi"
        };

        if (videoFiles.Count == 0)
        {
            Console.WriteLine("No video files found. Please add file paths.");
            return;
        }

        Console.WriteLine($"Starting {requestCount} parallel requests...");
        Stopwatch stopwatch = Stopwatch.StartNew();

        List<Task> tasks = new List<Task>();
        while (true)
        {
            for (int i = 0; i < requestCount; i++)
            {
                string randomFile = videoFiles[random.Next(videoFiles.Count)];
                tasks.Add(SendGifConversionRequest(apiUrl, randomFile, i + 1));
            }

            await Task.WhenAll(tasks);
            await Task.Delay(10000);
        }
        stopwatch.Stop();

        Console.WriteLine($"Completed {requestCount} requests in {stopwatch.ElapsedMilliseconds} ms");
    }

    static async Task SendGifConversionRequest(string apiUrl, string filePath, int requestId)
    {
        try
        {
            using var formData = new MultipartFormDataContent();
            using var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            using var fileContent = new StreamContent(fileStream);
            fileContent.Headers.ContentType = new MediaTypeHeaderValue("video/mp4");

            // Adding file content
            formData.Add(fileContent, "VideoFile", Path.GetFileName(filePath));

            // JSON settings
            var gifOptions = new
            {
                SetFps = false,
                Fps = 15,
                Width = 720,
                SetStartTime = false,
                StartTime = (TimeSpan?)null,
                SetEndTime = false,
                EndTime = (TimeSpan?)null,
                SetSpeed = false,
                SpeedMultiplier = 1.0,
                SetCrop = false,
                CropX = 0,
                CropY = 0,
                CropWidth = 0,
                CropHeight = 0,
                SetReverse = false,
                SetWatermark = false,
                WatermarkText = "",
                WatermarkFont = "",
                SetCompression = false,
                CompressionLevel = 0,
                SetReduceFrames = false,
                FrameSkipInterval = 2
            };

            var json = JsonSerializer.Serialize(gifOptions);
            var jsonContent = new StringContent(json, Encoding.UTF8, "application/json");

            formData.Add(jsonContent, "GifConversionOptions");

            // Send request
            Stopwatch timer = Stopwatch.StartNew();
            var response = await client.PostAsync(apiUrl, formData);
            timer.Stop();

            var responseBody = await response.Content.ReadAsStringAsync();
            Console.WriteLine($"Request {requestId} | File: {Path.GetFileName(filePath)} | Response: {response.StatusCode} | Time: {timer.ElapsedMilliseconds} ms");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Request {requestId} failed: {ex.Message}");
        }
    }
}
