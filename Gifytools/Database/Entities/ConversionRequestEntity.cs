using System.ComponentModel.DataAnnotations;
using System.Security.Principal;

namespace Gifytools.Database.Entities;
public class ConversionRequestEntity
{
    [Key]
    public Guid Id { get; set; }
    public required string VideoInputPath { get; set; }
    public string? GifOutputPath { get; set; }
    public DateTime CreateDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExecutedDate { get; set; }
    
    public bool SetFps { get; set; }
    public int Fps { get; set; }
    public int Width { get; set; }
    public bool SetStartTime { get; set; } 
    public TimeSpan? StartTime { get; set; }
    public bool SetEndTime { get; set; } 
    public TimeSpan? EndTime { get; set; }
    public bool SetSpeed { get; set; } 
    public double SpeedMultiplier { get; set; }
    public bool SetCrop { get; set; } 
    public int CropX { get; set; }
    public int CropY { get; set; }
    public int CropWidth { get; set; }
    public int CropHeight { get; set; }
    public bool SetReverse { get; set; }
    public bool SetWatermark { get; set; }
    public string? WatermarkText { get; set; }
    public string? WatermarkFont { get; set; }
    public bool SetCompression { get; set; } 
    public int CompressionLevel { get; set; } 
    public bool SetReduceFrames { get; set; } 
    public int FrameSkipInterval { get; set; }
    public ConversionStatusEnum ConversionStatus { get; set; } = ConversionStatusEnum.Pending;
    public string? ErrorMessage { get; set; }
    public string? IpAddress { get; set; }
}

public enum ConversionStatusEnum
{
    Pending = 1,
    Processing = 2,
    Completed = 3,
    Failed = 4
}

