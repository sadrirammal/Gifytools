using Gifytools.Database;
using Hangfire.Console;
using Hangfire.RecurringJobExtensions;
using Hangfire.Server;
using Microsoft.EntityFrameworkCore;
using Plantup.Hangfire.Hangfire;

namespace Gifytools.Jobs;

public class AutomaticDeletionJob : IRecurringJob
{

    private readonly AppDbContext _appDbContext;

    public AutomaticDeletionJob(AppDbContext appDbContext)
    {
        _appDbContext = appDbContext;
    }

    public void Execute(PerformContext context)
    {
        AsyncHelper.RunSync(() => ExecuteAsync(context));
    }

    public async Task ExecuteAsync(PerformContext context)
    {
        const int daysToKeep = 7;

        var conversionRequests = await _appDbContext.ConversionRequests
            .Where(x => x.CreateDate < DateTime.UtcNow.AddDays(-daysToKeep))
            .OrderByDescending(x => x.CreateDate)
            .ToListAsync();

        foreach(var conversionRequest in conversionRequests)
        {
            try
            {
                if(File.Exists(conversionRequest.VideoInputPath))
                {
                    File.Delete(conversionRequest.VideoInputPath);
                }

                if(!string.IsNullOrEmpty(conversionRequest.GifOutputPath) && File.Exists(conversionRequest.GifOutputPath))
                {
                    File.Delete(conversionRequest.GifOutputPath);
                }
            }
            catch(Exception ex)
            {
                context.WriteLine($"Failed to delete files for conversion request {conversionRequest.Id}. Because of {ex.Message} {ex.InnerException}");
            }

            _appDbContext.ConversionRequests.Remove(conversionRequest);
        }

        await _appDbContext.SaveChangesAsync();
    }
}
