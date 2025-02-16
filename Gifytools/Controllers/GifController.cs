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

    [HttpPost]
    public async Task<IActionResult> CreateGif(GifConversionOptions options)
    {
        if (options.VideoFile == null || options.VideoFile.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        // Save uploaded video to file system
        var path = await _videoToGifService.UploadVideo(options.VideoFile);

        // Convert video to gif
        try
        {
            await _videoToGifService.ConvertToGif(path, options.VideoFile.Name, options);
        }
        catch (Exception ex)
        {
            Console.WriteLine("An error occurred: " + ex.Message);
        }
        return Ok();
    }
}