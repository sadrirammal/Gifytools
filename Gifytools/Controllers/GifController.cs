using Gifytools.Bll;
using Gifytools.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Gifytools.Controllers;
[Route("api/[controller]")]
[ApiController]
public class GifController : ControllerBase
{
    private readonly IVideoToGifService _videoToGifService;

    public GifController(IVideoToGifService videoToGifService)
    {
        _videoToGifService = videoToGifService;
    }

    [HttpPost("videoToGif")]
    public async Task<IActionResult> CreateGif([FromForm] GifConversionOptions options)
    {
        if (options.VideoFile == null || options.VideoFile.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        // Save uploaded video to file system
        var path = await _videoToGifService.UploadVideo(options.VideoFile);

        // Convert video to gif
        string gifPath;
        try
        {
            gifPath = await _videoToGifService.ConvertToGif(path, options.VideoFile.Name, options);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
            return StatusCode(500, "Error processing the GIF.");
        }

        // Check if the GIF file was successfully created
        if (!System.IO.File.Exists(gifPath))
        {
            return StatusCode(500, "GIF generation failed.");
        }

        var fileStream = new FileStream(gifPath, FileMode.Open, FileAccess.Read);
        var fileType = "image/gif";
        var fileName = Path.GetFileName(gifPath);

        return File(fileStream, fileType, fileName);
    }
}