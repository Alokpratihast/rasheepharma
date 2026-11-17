using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Domain.Enums;

namespace RashePharma.Infrastructure.Workers;

public class BulkUploadWorker : BackgroundService
{
    private const int MaxJobsPerCycle = 1;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BulkUploadWorker> _logger;
    private readonly TimeSpan _idlePollingInterval;
    private readonly TimeSpan _activePollingInterval;

    public BulkUploadWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<BulkUploadWorker> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _idlePollingInterval = TimeSpan.FromSeconds(
            Math.Clamp(configuration.GetValue("BulkUploadWorker:IdlePollingIntervalSeconds", 60), 5, 600));
        _activePollingInterval = TimeSpan.FromMilliseconds(
            Math.Clamp(configuration.GetValue("BulkUploadWorker:ActivePollingIntervalMilliseconds", 200), 100, 5000));
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Bulk upload worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var processed = await ProcessPendingJobsAsync(stoppingToken);
                await Task.Delay(
                    processed ? _activePollingInterval : _idlePollingInterval,
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error occurred in bulk upload worker.");

                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        _logger.LogInformation(
            "Bulk upload worker stopped.");
    }


private async Task<bool> ProcessPendingJobsAsync(
    CancellationToken cancellationToken)
{
    for (var i = 0; i < MaxJobsPerCycle; i++)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var scope = _scopeFactory.CreateScope();

        var repository =
            scope.ServiceProvider
                .GetRequiredService<IBulkUploadRepository>();

        var service =
            scope.ServiceProvider
                .GetRequiredService<IBulkUploadService>();

        var job = await repository.ClaimNextPendingJobAsync(
            cancellationToken);

        if (job is null)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(job.ClaimToken))
        {
            throw new InvalidOperationException(
                $"Claim token is missing for bulk upload job {job.Id}.");
        }

        var claimToken = job.ClaimToken;

        _logger.LogInformation(
            "Bulk upload job {JobId} claimed. Attempt {AttemptCount}.",
            job.Id,
            job.AttemptCount);

        using var processingCts =
            CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken);

        using var heartbeatCts =
            CancellationTokenSource.CreateLinkedTokenSource(
                cancellationToken);

        var heartbeatTask = RenewLeaseLoopAsync(
            job.Id,
            claimToken,
            heartbeatCts.Token,
            processingCts);

        try
        {
            await service.ProcessAsync(
                job.Id,
                claimToken,
                processingCts.Token);

            _logger.LogInformation(
                "Bulk upload job {JobId} processing completed.",
                job.Id);
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException)
            when (processingCts.IsCancellationRequested)
        {
            _logger.LogWarning(
                "Bulk upload job {JobId} processing stopped because its lease was lost or processing was cancelled.",
                job.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Bulk upload job {JobId} failed during processing.",
                job.Id);
        }
        finally
        {
            await heartbeatCts.CancelAsync();

            try
            {
                await heartbeatTask;
            }
            catch (OperationCanceledException)
                when (heartbeatCts.IsCancellationRequested)
            {
                // Expected when processing completes or stops.
            }
        }
    }

    return true;
}

private async Task RenewLeaseLoopAsync(
    int jobId,
    string claimToken,
    CancellationToken heartbeatToken,
    CancellationTokenSource processingCts)
{
    var leaseDuration = TimeSpan.FromMinutes(2);
    var renewalInterval = TimeSpan.FromSeconds(30);

    while (true)
    {
        await Task.Delay(renewalInterval, heartbeatToken);

        heartbeatToken.ThrowIfCancellationRequested();

        using var scope = _scopeFactory.CreateScope();

        var repository =
            scope.ServiceProvider
                .GetRequiredService<IBulkUploadRepository>();

        var renewed = await repository.TryRenewLeaseAsync(
            jobId,
            claimToken,
            leaseDuration,
            heartbeatToken);

        if (renewed)
        {
            _logger.LogDebug(
                "Lease renewed for bulk upload job {JobId}.",
                jobId);

            continue;
        }

        _logger.LogWarning(
            "Lease renewal failed for bulk upload job {JobId}. Processing will be cancelled.",
            jobId);

        await processingCts.CancelAsync();
        return;
    }
}

}