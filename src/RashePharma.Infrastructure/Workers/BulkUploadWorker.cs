using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Infrastructure.Services;

namespace RashePharma.Infrastructure.Workers;

/// <summary>
/// Event-driven bulk-upload processor. SQL claim/lease semantics remain the
/// concurrency authority; periodic reconciliation recovers missed signals.
/// </summary>
public sealed class BulkUploadWorker : BackgroundService
{
    private static readonly TimeSpan RecoveryInterval = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan ErrorRetryDelay = TimeSpan.FromSeconds(15);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IBulkUploadJobSignal _jobSignal;
    private readonly ILogger<BulkUploadWorker> _logger;

    public BulkUploadWorker(
        IServiceScopeFactory scopeFactory,
        IBulkUploadJobSignal jobSignal,
        ILogger<BulkUploadWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _jobSignal = jobSignal;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Event-driven bulk upload worker started.");

        try
        {
            // Startup reconciliation also recovers jobs left behind by a restart.
            await DrainPendingJobsSafelyAsync(stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                var signaled = await _jobSignal.WaitForJobAsync(
                    RecoveryInterval,
                    stoppingToken);

                if (signaled)
                    _logger.LogDebug("Bulk upload worker received a job signal.");
                else
                    _logger.LogDebug("Bulk upload worker starting periodic recovery scan.");

                await DrainPendingJobsSafelyAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Normal host shutdown.
        }
        finally
        {
            _logger.LogInformation("Bulk upload worker stopped.");
        }
    }

    private async Task DrainPendingJobsSafelyAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                var processed = await ProcessOnePendingJobAsync(cancellationToken);
                if (!processed)
                    return;
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                // A transient SQL/repository failure must not terminate the hosted service.
                _logger.LogError(exception, "Bulk upload worker failed while claiming or processing a job. Retrying after {RetryDelaySeconds} seconds.", ErrorRetryDelay.TotalSeconds);
                await Task.Delay(ErrorRetryDelay, cancellationToken);
            }
        }
    }

    private async Task<bool> ProcessOnePendingJobAsync(CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBulkUploadRepository>();
        var service = scope.ServiceProvider.GetRequiredService<IBulkUploadService>();

        var job = await repository.ClaimNextPendingJobAsync(cancellationToken);
        if (job is null)
            return false;

        if (string.IsNullOrWhiteSpace(job.ClaimToken))
            throw new InvalidOperationException($"Claim token is missing for bulk upload job {job.Id}.");

        var claimToken = job.ClaimToken;
        _logger.LogInformation(
            "Bulk upload job {JobId} claimed. Attempt {AttemptCount}.",
            job.Id,
            job.AttemptCount);

        using var processingCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        using var heartbeatCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var heartbeatTask = RenewLeaseLoopAsync(
            job.Id,
            claimToken,
            heartbeatCts.Token,
            processingCts);

        try
        {
            await service.ProcessAsync(job.Id, claimToken, processingCts.Token);
            _logger.LogInformation("Bulk upload job {JobId} processing completed.", job.Id);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException) when (processingCts.IsCancellationRequested)
        {
            _logger.LogWarning(
                "Bulk upload job {JobId} stopped because its lease was lost or processing was cancelled.",
                job.Id);
        }
        catch (Exception exception)
        {
            // ProcessAsync owns the job-level failure transition. Keep the worker alive.
            _logger.LogError(exception, "Bulk upload job {JobId} failed during processing.", job.Id);
        }
        finally
        {
            await heartbeatCts.CancelAsync();
            try
            {
                await heartbeatTask;
            }
            catch (OperationCanceledException) when (heartbeatCts.IsCancellationRequested)
            {
                // Expected when processing completes.
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Lease heartbeat failed for bulk upload job {JobId}.", job.Id);
                await processingCts.CancelAsync();
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
            var repository = scope.ServiceProvider.GetRequiredService<IBulkUploadRepository>();
            var renewed = await repository.TryRenewLeaseAsync(
                jobId,
                claimToken,
                leaseDuration,
                heartbeatToken);

            if (renewed)
                continue;

            _logger.LogWarning(
                "Lease renewal failed for bulk upload job {JobId}; cancelling processing.",
                jobId);
            await processingCts.CancelAsync();
            return;
        }
    }
}
