export interface GifConversionOptions {
    // Frame Rate
    SetFps: boolean;
    Fps: number;
  
    // Width & Height (Aspect Ratio Preservation)
    Width: number;
  
    // Duration (Start & End Time)
    SetStartTime: boolean;
    StartTime?: string | null; // Using string to match JSON serialization (e.g., "00:01:30")
    
    SetEndTime: boolean;
    EndTime?: string | null; // Using string for compatibility
  
    // Speed Adjustment
    SetSpeed: boolean;
    SpeedMultiplier: number;
  
    // Crop & Trim
    SetCrop: boolean;
    CropX: number;
    CropY: number;
    CropWidth: number;
    CropHeight: number;
  
    // Reverse GIF
    SetReverse: boolean;
  
    // Text / Watermark
    SetWatermark: boolean;
    WatermarkText?: string;
    WatermarkFont?: string;
  
    // Compression Level (Quality Optimization)
    SetCompression: boolean;
    CompressionLevel: number;
  
    // Reduce Frames (Skip frames to reduce file size)
    SetReduceFrames: boolean;
    FrameSkipInterval: number;
  
    // Video File (Not sent in JSON but used for form data)
    VideoFile?: File;
  }
  