using Gifytools.Bll;
using Gifytools.Database;
using Gifytools.Database.Entities;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace Gifytools.ProcessQueue;
public class GifConversionWorker
{
    private readonly AppDbContext _appDbContext;
    private readonly IVideoToGifService _videoToGif;

    public GifConversionWorker(AppDbContext appDbContext, IVideoToGifService videoToGif)
    {
        _appDbContext = appDbContext;
        _videoToGif = videoToGif;
    }
    
    public async Task Execute(Guid conversionRequestId)
    {
        var conversionRequest = await _appDbContext.ConversionRequests.FirstAsync(x => x.Id == conversionRequestId);

        try
        {
            var gifPath = await _videoToGif.ConvertToGif(conversionRequest);
            conversionRequest.GifOutputPath = gifPath;
            conversionRequest.ConversionStatus = ConversionStatusEnum.Completed;
        }
        catch (Exception e)
        {
            conversionRequest.ConversionStatus = ConversionStatusEnum.Failed;
            conversionRequest.ErrorMessage = e.Message;
        }

        conversionRequest.ExecutedDate = DateTime.UtcNow;
        
        await _appDbContext.SaveChangesAsync();
    }
}
