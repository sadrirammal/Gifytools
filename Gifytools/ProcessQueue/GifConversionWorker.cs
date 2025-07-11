using Gifytools.Bll;
using Gifytools.Database;
using Gifytools.Database.Entities;
using Gifytools.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace Gifytools.ProcessQueue;
public class GifConversionWorker
{
    private readonly AppDbContext _appDbContext;
    private readonly IVideoToGifService _videoToGif;
    private readonly GifyMetrics _metrics;
    private readonly ILogger<GifConversionWorker> _logger;

    public GifConversionWorker(AppDbContext appDbContext, IVideoToGifService videoToGif, GifyMetrics metrics, ILogger<GifConversionWorker> logger)
    {
        _appDbContext = appDbContext;
        _videoToGif = videoToGif;
        _metrics = metrics;
        _logger = logger;
    }
    
    public async Task Execute(Guid conversionRequestId)
    {
        using var activity = GifyMetrics.GifytoolsActivitySource.StartActivity("Gifytools.Convert");
        activity?.SetCustomProperty(nameof(conversionRequestId), conversionRequestId);
        _logger.LogInformation("Started conversion with request id {conversionRequest}", conversionRequestId);
        var conversionRequest = await _appDbContext.ConversionRequests.FirstAsync(x => x.Id == conversionRequestId);
        _metrics.JobStarted();
        try
        {
            var gifPath = await _videoToGif.ConvertToGif(conversionRequest);
            conversionRequest.GifOutputPath = gifPath;
            conversionRequest.ConversionStatus = ConversionStatusEnum.Completed;
            _logger.LogInformation("Conversion {conversionRequest} done", conversionRequest);
            _metrics.GifCreated();
            activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Ok);
        }
        catch (Exception e)
        {
            conversionRequest.ConversionStatus = ConversionStatusEnum.Failed;
            conversionRequest.ErrorMessage = e.Message;
            _logger.LogError(e, "Error while converting video for request {conversionRequest}", conversionRequest);
            activity?.SetStatus(System.Diagnostics.ActivityStatusCode.Error);
        }
        _metrics.JobFinished();
        conversionRequest.ExecutedDate = DateTime.UtcNow;
        
        await _appDbContext.SaveChangesAsync();
    }
}
