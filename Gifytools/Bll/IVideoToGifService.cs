using Gifytools.Database.Entities;

namespace Gifytools.Bll;

public interface IVideoToGifService
{
    Task<string> UploadVideo(IFormFile videoFile);

    Task<string> ConvertToGif(ConversionRequestEntity entity);

}