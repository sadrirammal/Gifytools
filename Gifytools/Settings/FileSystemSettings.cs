namespace Gifytools.Settings;

public class FileSystemSettings
{
    public required string FFmpegPath { get; set; }
    public required string VideoInputPath { get; set; }
    public required string GifOutputPath { get; set; }
    public List<FontSettings> Fonts { get; set; } = new List<FontSettings>();
}

public class FontSettings
{
    public string Name { get; set; }
    public string Path { get; set; }
}