using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Domain.Entities.BulkUpload;
using RashePharma.Infrastructure.Configurations;

namespace RashePharma.Infrastructure.Services;

public sealed class BulkUploadDispatchOutboxDispatcher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly AzureServiceBusBulkUploadPublisher _publisher;
    private readonly ILogger<BulkUploadDispatchOutboxDispatcher> _logger;
    private readonly OutboxDispatcherOptions _options;

    public BulkUploadDispatchOutboxDispatcher(IServiceScopeFactory scopeFactory,
        AzureServiceBusBulkUploadPublisher publisher, IOptions<OutboxDispatcherOptions> options,
        ILogger<BulkUploadDispatchOutboxDispatcher> logger)
    {
        _scopeFactory = scopeFactory; _publisher = publisher; _options = options.Value; _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var count = await DispatchBatchAsync(stoppingToken);
                if (count >= _options.BatchSize) continue;
                await Task.Delay(TimeSpan.FromSeconds(_options.PollingIntervalSeconds), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { break; }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Bulk-upload dispatch outbox loop failed.");
                await Task.Delay(TimeSpan.FromSeconds(15), stoppingToken);
            }
        }
    }

    private async Task<int> DispatchBatchAsync(CancellationToken ct)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var repo = scope.ServiceProvider.GetRequiredService<IBulkUploadDispatchOutboxRepository>();
        var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        var messages = await repo.ClaimPendingAsync(_options.BatchSize, DateTime.UtcNow,
            TimeSpan.FromSeconds(_options.LeaseDurationSeconds), ct);
        foreach (var message in messages)
        {
            try
            {
                await _publisher.PublishAsync(message, ct);
                message.Status = BulkUploadDispatchStatus.Published;
                message.PublishedAtUtc = DateTime.UtcNow;
                message.LeaseUntilUtc = null;
                message.NextAttemptAtUtc = null;
                message.LastError = null;
            }
            catch (OperationCanceledException) when (ct.IsCancellationRequested) { throw; }
            catch (Exception ex)
            {
                message.LastError = ex.Message.Length > 2000 ? ex.Message[..2000] : ex.Message;
                message.LeaseUntilUtc = null;
                if (ex is ServiceBusException sb && !sb.IsTransient || message.AttemptCount >= _options.MaxAttempts)
                {
                    message.Status = BulkUploadDispatchStatus.DeadLettered;
                    message.NextAttemptAtUtc = null;
                }
                else
                {
                    message.Status = BulkUploadDispatchStatus.RetryScheduled;
                    var seconds = Math.Min(_options.RetryBaseDelaySeconds * Math.Pow(2, Math.Max(0, message.AttemptCount - 1)),
                        _options.RetryMaxDelaySeconds);
                    message.NextAttemptAtUtc = DateTime.UtcNow.AddSeconds(seconds);
                }
                _logger.LogWarning(ex, "Bulk-upload dispatch {OutboxId} attempt {Attempt} failed.",
                    message.Id, message.AttemptCount);
            }
            await uow.SaveChangesAsync();
        }
        return messages.Count;
    }
}
