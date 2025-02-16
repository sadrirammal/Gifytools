namespace Gifytools.Bll;

public class GifConversionOptions
{
    // Frame Rate
    public bool SetFps { get; set; } = false;
    public int Fps { get; set; } = 15;

    // Width & Height (Aspect Ratio Preservation)
    public int Width { get; set; } = 720;

    // Duration (Start & End Time)
    public bool SetStartTime { get; set; } = false;
    public TimeSpan? StartTime { get; set; }

    public bool SetEndTime { get; set; } = false;
    public TimeSpan? EndTime { get; set; }

    // Speed Adjustment
    public bool SetSpeed { get; set; } = false;
    public double SpeedMultiplier { get; set; } = 1.0; // 1.0 = normal speed

    // Crop & Trim
    public bool SetCrop { get; set; } = false;
    public int CropX { get; set; }
    public int CropY { get; set; }
    public int CropWidth { get; set; }
    public int CropHeight { get; set; }

    // Reverse GIF
    public bool SetReverse { get; set; } = false;

    // Text / Watermark
    public bool SetWatermark { get; set; } = false;
    public string WatermarkText { get; set; }
    public string? WatermarkFont { get; set; }

    // Compression Level (Quality Optimization)
    public bool SetCompression { get; set; } = false;
    public int CompressionLevel { get; set; } = 0; // 0-100 scale

    // Reduce Frames (Skip frames to reduce file size)
    public bool SetReduceFrames { get; set; } = false;
    public int FrameSkipInterval { get; set; } = 2; // Example: 2 means take every 2nd frame

    public IFormFile? VideoFile { get; set; }
}