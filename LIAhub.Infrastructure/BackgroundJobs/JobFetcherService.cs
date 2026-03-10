using LIAhub.Infrastructure.Data;
using LIAhub.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LIAhub.Infrastructure.BackgroundJobs;

public class JobFetcherService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<JobFetcherService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromHours(6);

    public JobFetcherService(IServiceScopeFactory scopeFactory, ILogger<JobFetcherService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("JobFetcherService started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            await FetchAndStoreJobsAsync();
            await Task.Delay(_interval, stoppingToken);
        }
    }

    private async Task FetchAndStoreJobsAsync()
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var jobSearchService = scope.ServiceProvider.GetRequiredService<JobSearchService>();

            _logger.LogInformation("Fetching jobs from JobSearch API...");

            var jobs = await jobSearchService.FetchLiaJobsAsync();

            // Remove expired jobs
            var expired = await db.CachedJobs
                .Where(j => j.ExpiresAt < DateTime.UtcNow)
                .ToListAsync();
            db.CachedJobs.RemoveRange(expired);

            // Add new jobs if they don't already exist
            foreach (var job in jobs)
            {
                var exists = await db.CachedJobs
                    .AnyAsync(j => j.ExternalId == job.ExternalId);

                if (!exists)
                    await db.CachedJobs.AddAsync(job);
            }

            await db.SaveChangesAsync();
            _logger.LogInformation("Fetched and stored {Count} jobs.", jobs.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching jobs.");
        }
    }
}