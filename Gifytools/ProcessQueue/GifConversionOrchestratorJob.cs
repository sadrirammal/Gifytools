using Gifytools.Database;
using Hangfire;
using Hangfire.RecurringJobExtensions;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;

namespace Gifytools.ProcessQueue;
public class GifConversionOrchestratorJob : IRecurringJob
{
    private readonly AppDbContext _appDbContext;

    public GifConversionOrchestratorJob(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }
    
    public void Execute(PerformContext context)
    {
        var openConversions = _appDbContext.ConversionRequests.Where(x => x.ExecutedDate == null && x.ErrorMessage == null)
            .OrderBy(x => x.CreateDate)
            .AsNoTracking()
            .Take(5)
            .ToList();
        
        foreach (var conversionRequest in openConversions)
        {
            BackgroundJob.Enqueue<GifConversionWorker>(worker => worker.Execute(conversionRequest.Id));
        }

    }
}
