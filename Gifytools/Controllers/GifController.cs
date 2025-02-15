using Gifytools.Bll;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Gifytools.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GifController : ControllerBase
    {
        [HttpGet]
        public IActionResult Get()
        {
            try
            {
                // Path to the FFmpeg executable
                string ffmpegPath = "C:\\ffmpeg\\bin\\ffmpeg.exe";

                // Initialize the VideoToGif converter
                VideoToGif converter = new VideoToGif(ffmpegPath);

                // Input and output paths
                string inputVideo = "C:\\data\\input.avi";
                string outputGif = "C:\\data\\output.gif";

                // Convert video to GIF
                converter.ConvertToGif(inputVideo, outputGif);

                Console.WriteLine("Conversion successful! GIF saved at: " + outputGif);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            return Ok();
        }

        [HttpGet("text")]
        public IActionResult Get2()
        {
            try
            {
                // Path to the FFmpeg executable
                string ffmpegPath = "C:\\ffmpeg\\bin\\ffmpeg.exe";
                string inputGif = "C:\\data\\output.gif";
                string outputGif = "C:\\data\\outputWithText.gif";

                // Initialize the VideoToGif converter
                VideoToGif converter = new VideoToGif(ffmpegPath);

                // Convert video to GIF
                converter.AddTextToGif(inputGif, outputGif, "Plantup Watertank", 3, 720, 1080);

                Console.WriteLine("Conversion successful! GIF saved at: " + outputGif);
            }
            catch (Exception ex)
            {
                Console.WriteLine("An error occurred: " + ex.Message);
            }
            return Ok();
        }
    }
}
