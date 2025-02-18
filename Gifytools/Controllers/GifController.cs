using Gifytools.Bll;
using Gifytools.Database;
using Gifytools.Database.Entities;
using Gifytools.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Gifytools.Controllers;
[Route("api/[controller]")]
[ApiController]
public class GifController : ControllerBase
{
    private readonly IVideoToGifService _videoToGifService;
    private readonly AppDbContext _appDbContext;

    public GifController(IVideoToGifService videoToGifService, AppDbContext appDbContext)
    {
        _videoToGifService = videoToGifService;
        _appDbContext = appDbContext;
    }

    [HttpPost("ConversionRequest")]
    public async Task<ActionResult<Guid>> ScheduleGif([FromForm] GifConversionOptions options)
    {
        if (options.VideoFile == null || options.VideoFile.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        var path = await _videoToGifService.UploadVideo(options.VideoFile);

        var entity = new ConversionRequestEntity
        {
            VideoInputPath = path,
            SetFps = options.SetFps,
            Fps = options.Fps,
            Width = options.Width,
            SetStartTime = options.SetStartTime,
            StartTime = options.StartTime,
            SetEndTime = options.SetEndTime,
            EndTime = options.EndTime,
            SetSpeed = options.SetSpeed,
            SpeedMultiplier = options.SpeedMultiplier,
            SetCrop = options.SetCrop,
            CropX = options.CropX,
            CropY = options.CropY,
            CropWidth = options.CropWidth,
            CropHeight = options.CropHeight,
            SetReverse = options.SetReverse,
            SetWatermark = options.SetWatermark,
            WatermarkText = options.WatermarkText,
            WatermarkFont = options.WatermarkFont,
            SetCompression = options.SetCompression,
            CompressionLevel = options.CompressionLevel,
            SetReduceFrames = options.SetReduceFrames,
            FrameSkipInterval = options.FrameSkipInterval,
            ConversionStatus = ConversionStatusEnum.Pending
        };

        await _appDbContext.ConversionRequests.AddAsync(entity);
        
        await _appDbContext.SaveChangesAsync();
        return Ok(entity.Id);
    }

    [HttpGet("ConversionRequest/{id}/status")]
    public async Task<ActionResult<object>> GetConversionStatus(Guid id)
    {
        var request = await _appDbContext.ConversionRequests
            .Where(r => r.Id == id)
            .Select(r => new
            {
                r.Id,
                r.ConversionStatus,
            })
            .FirstOrDefaultAsync();

        if (request == null)
            return NotFound();

        return Ok(request);
    }

    [HttpGet("ConversionRequest/{id}/download")]
    public async Task<IActionResult> DownloadGif(Guid id)
    {
        var request = await _appDbContext.ConversionRequests
            .Where(r => r.Id == id && r.ConversionStatus == ConversionStatusEnum.Completed)
            .FirstOrDefaultAsync();

        if (request == null || string.IsNullOrEmpty(request.GifOutputPath))
        {
            return NotFound("Filepath not found or conversion not completed.");
        }

        var filePath = request.GifOutputPath;

        if (!System.IO.File.Exists(filePath))
        {
            return NotFound("GIF file not found on the server.");
        }

        var memory = new MemoryStream();
        await using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        {
            await stream.CopyToAsync(memory);
        }
        memory.Position = 0;

        var contentType = "image/gif"; // Ensure correct content type
        var fileName = Path.GetFileName(filePath);

        return File(memory, contentType, fileName);
    }


    //[HttpGet("ConversionRequest")]
    //public async Task<ActionResult<List<ConversionRequestEntity>>> GetConversionRequests()
    //{
    //    return await _appDbContext.ConversionRequests.ToListAsync();
    //}

    //TODO: remove once all logic is somewhere else
    //[HttpPost("videoToGif")]
    //public async Task<IActionResult> CreateGif([FromForm] GifConversionOptions options)
    //{
    //    if (options.VideoFile == null || options.VideoFile.Length == 0)
    //    {
    //        return BadRequest("No file uploaded.");
    //    }

    //    // Save uploaded video to file system
    //    var path = await _videoToGifService.UploadVideo(options.VideoFile);

    //    // Convert video to gif
    //    string gifPath;
    //    try
    //    {
    //        gifPath = await _videoToGifService.ConvertToGif(path, options.VideoFile.Name, options);
    //    }
    //    catch (Exception ex)
    //    {
    //        Console.WriteLine("An error occurred: " + ex.Message);
    //        return StatusCode(500, "Error processing the GIF.");
    //    }

    //    // Check if the GIF file was successfully created
    //    if (!System.IO.File.Exists(gifPath))
    //    {
    //        return StatusCode(500, "GIF generation failed.");
    //    }

    //    var fileStream = new FileStream(gifPath, FileMode.Open, FileAccess.Read);
    //    var fileType = "image/gif";
    //    var fileName = Path.GetFileName(gifPath);

    //    return File(fileStream, fileType, fileName);
    //}
}