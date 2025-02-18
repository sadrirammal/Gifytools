using Gifytools.Bll;
using Gifytools.Database;
using Gifytools.Database.Entities;
using Microsoft.EntityFrameworkCore;

namespace Gifytools.ProcessQueue;
public class GifConversionBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public GifConversionBackgroundService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            int processedCount = await ProcessConversionRequests(stoppingToken);

            // Adjust delay dynamically: If we processed something, retry sooner; if nothing, wait longer.
            int delayMs = processedCount > 0 ? 5 * 1000 : 15 * 1000; // 5s if work was done, else 60s
            await Task.Delay(delayMs, stoppingToken);
        }
    }

    private async Task<int> ProcessConversionRequests(CancellationToken stoppingToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Fetch & Update status within a single transaction
        var conversionRequests = await dbContext.ConversionRequests
            .Where(x => x.ConversionStatus == ConversionStatusEnum.Pending)
            .OrderBy(x => x.CreateDate)
            .Take(5) // Max 5 concurrent conversions due to server limits
            .ToListAsync(stoppingToken);

        if (!conversionRequests.Any())
            return 0;

        conversionRequests.ForEach(x => x.ConversionStatus = ConversionStatusEnum.Processing);
        await dbContext.SaveChangesAsync(stoppingToken); // Save status update

        // Run conversions in parallel
        List<Task> tasks = conversionRequests.Select(Convert).ToList();
        await Task.WhenAll(tasks);

        await dbContext.SaveChangesAsync(stoppingToken); // Save status update

        return conversionRequests.Count;
    }

    private async Task Convert(ConversionRequestEntity conversionRequest)
    {
        using var scope = _scopeFactory.CreateScope();
        var videoToGifService = scope.ServiceProvider.GetRequiredService<IVideoToGifService>();

        try
        {
            var gifPath = await videoToGifService.ConvertToGif(conversionRequest);
            conversionRequest.GifOutputPath = gifPath;
            conversionRequest.ConversionStatus = ConversionStatusEnum.Completed;
        }
        catch (Exception e)
        {
            conversionRequest.ConversionStatus = ConversionStatusEnum.Failed;
            conversionRequest.ErrorMessage = e.Message;
        }

        conversionRequest.ExecutedDate = DateTime.UtcNow;
    }
}
