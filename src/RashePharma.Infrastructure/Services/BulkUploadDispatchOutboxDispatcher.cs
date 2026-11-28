using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Domain.Entities.BulkUpload;
using RashePharma.Infrastructure.Configurations;

namespace RashePharma.Infrastructure.Services;

public sealed class BulkUploadDispatchOutboxDispatcher : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly IBulkUploadDispatchPublisher _publisher;
    private readonly ILogger<BulkUploadDispatchOutboxDispatcher> _logger;
    private readonly BulkUploadDispatchOptions _options;
    private readonly IBulkUploadDispatchSignal _signal;

    public BulkUploadDispatchOutboxDispatcher(
        IServiceScopeFactory scopes,
        IBulkUploadDispatchPublisher publisher,
        IOptions<BulkUploadDispatchOptions> options,
        ILogger<BulkUploadDispatchOutboxDispatcher> logger,
        IBulkUploadDispatchSignal signal)
    {
        _scopes = scopes;
        _publisher = publisher;
        _options = options.Value;
        _logger = logger;
        _signal = signal;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = await DispatchAsync(stoppingToken);
                if (result.Count >= _options.BatchSize)
                    continue;

                var wait = CalculateWaitDuration(
                    result.NextRetryAtUtc,
                    DateTime.UtcNow,
                    TimeSpan.FromSeconds(_options.RecoveryIntervalSeconds));

                await _signal.WaitForMessageAsync(wait, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bulk upload outbox dispatch failed.");
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
        }
    }

    public static TimeSpan CalculateWaitDuration(
        DateTime? nextRetryAtUtc,
        DateTime nowUtc,
        TimeSpan? recoveryInterval = null)
    {
        if (nextRetryAtUtc is null)
            return recoveryInterval ?? Timeout.InfiniteTimeSpan;

        var untilRetry = nextRetryAtUtc.Value - nowUtc;
        if (untilRetry <= TimeSpan.Zero)
            return TimeSpan.Zero;
        if (recoveryInterval is { } interval && untilRetry > interval)
            return interval;
        return untilRetry;
    }

    private async Task<(int Count, DateTime? NextRetryAtUtc)> DispatchAsync(CancellationToken ct)
    {
        await using var scope = _scopes.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IBulkUploadDispatchOutboxRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        await repo.RequeueExpiredProcessingJobsAsync(DateTime.UtcNow, ct);
        var batch = await repo.ClaimPendingAsync(
            _options.BatchSize,
            DateTime.UtcNow,
            TimeSpan.FromSeconds(_options.LeaseDurationSeconds),
            ct);

        foreach (var item in batch)
        {
            try
            {
                await _publisher.PublishAsync(item, ct);
                item.Status = BulkUploadDispatchStatus.Published;
                item.PublishedAtUtc = DateTime.UtcNow;
                item.LeaseUntilUtc = null;
                item.NextAttemptAtUtc = null;
                item.LastError = null;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                item.LastError = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;
                item.LeaseUntilUtc = null;

                if (item.AttemptCount >= _options.MaxAttempts)
                {
                    item.Status = BulkUploadDispatchStatus.DeadLettered;
                    item.NextAttemptAtUtc = null;
                }
                else
                {
                    item.Status = BulkUploadDispatchStatus.RetryScheduled;
                    var delay = Math.Min(
                        _options.RetryBaseDelaySeconds * Math.Pow(2, Math.Max(0, item.AttemptCount - 1)),
                        _options.RetryMaxDelaySeconds);
                    item.NextAttemptAtUtc = DateTime.UtcNow.AddSeconds(delay);

                }

                _logger.LogWarning(
                    ex,
                    "Bulk upload outbox message {Id} attempt {Attempt} failed.",
                    item.Id,
                    item.AttemptCount);
            }

            await uow.SaveChangesAsync();
        }

        // Query SQL even when this batch is empty: a previous full batch may
        // have left RetryScheduled rows whose due time is still in the future.
        var nextRetryAtUtc = await repo.GetNextRetryAtUtcAsync(ct);
        return (batch.Count, nextRetryAtUtc);
    }
}
