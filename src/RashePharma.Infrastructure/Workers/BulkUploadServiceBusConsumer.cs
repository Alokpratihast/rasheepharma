using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Domain.Enums;
using RashePharma.Infrastructure.Configurations;

namespace RashePharma.Infrastructure.Workers;

public sealed class BulkUploadServiceBusConsumer : BackgroundService
{
    private static readonly TimeSpan JobLeaseDuration = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan HeartbeatInterval = TimeSpan.FromSeconds(30);

    private readonly ServiceBusProcessor _processor;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BulkUploadServiceBusConsumer> _logger;

    public BulkUploadServiceBusConsumer(
        ServiceBusClient client,
        IOptions<ServiceBusOptions> options,
        IServiceScopeFactory scopeFactory,
        ILogger<BulkUploadServiceBusConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _processor = client.CreateProcessor(
            options.Value.BulkUploadQueueName,
            new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 2,
                MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(5),
                PrefetchCount = 0
            });
        _processor.ProcessMessageAsync += HandleMessageAsync;
        _processor.ProcessErrorAsync += HandleErrorAsync;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _processor.StartProcessingAsync(stoppingToken);
        try
        {
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Host is stopping.
        }
        finally
        {
            await _processor.StopProcessingAsync(CancellationToken.None);
        }
    }

    private async Task HandleMessageAsync(ProcessMessageEventArgs args)
    {
        if (!args.Message.ApplicationProperties.TryGetValue("BulkUploadJobId", out var rawId) ||
            !int.TryParse(Convert.ToString(rawId), out var jobId) ||
            jobId <= 0)
        {
            await args.DeadLetterMessageAsync(
                args.Message,
                "InvalidBulkUploadJobId",
                "The message does not contain a valid BulkUploadJobId.",
                args.CancellationToken);
            return;
        }

        await using var scope = _scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBulkUploadRepository>();
        var service = scope.ServiceProvider.GetRequiredService<IBulkUploadService>();

        var job = await repository.ClaimJobAsync(jobId, args.CancellationToken);
        if (job is null)
        {
            // A duplicate delivery may arrive while another worker owns the lease.
            // Terminal jobs are also safe to acknowledge. Pending jobs are not.
            var current = await repository.GetByIdAsync(jobId, args.CancellationToken);
            if (current is null)
            {
                await args.DeadLetterMessageAsync(
                    args.Message,
                    "BulkUploadJobNotFound",
                    $"Bulk upload job {jobId} does not exist.",
                    args.CancellationToken);
                return;
            }

            if (current.Status == BulkUploadStatus.Pending)
            {
                _logger.LogWarning(
                    "Bulk upload job {JobId} was not claimable while still Pending; abandoning for redelivery.",
                    jobId);
                await args.AbandonMessageAsync(args.Message, cancellationToken: args.CancellationToken);
                return;
            }

            await args.CompleteMessageAsync(args.Message, args.CancellationToken);
            return;
        }

        if (string.IsNullOrWhiteSpace(job.ClaimToken))
        {
            throw new InvalidOperationException(
                $"Claim token missing for bulk upload job {jobId}.");
        }

        using var processingCts =
            CancellationTokenSource.CreateLinkedTokenSource(args.CancellationToken);
        using var heartbeatCts =
            CancellationTokenSource.CreateLinkedTokenSource(args.CancellationToken);

        var heartbeat = RenewLeaseLoopAsync(
            job.Id,
            job.ClaimToken,
            heartbeatCts.Token,
            processingCts);

        try
        {
            await service.ProcessAsync(job.Id, job.ClaimToken, processingCts.Token);
            await args.CompleteMessageAsync(args.Message, args.CancellationToken);
        }
        catch (OperationCanceledException) when (args.CancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException) when (processingCts.IsCancellationRequested)
        {
            _logger.LogWarning(
                "Bulk upload job {JobId} stopped because lease ownership could not be maintained; abandoning message.",
                job.Id);
            await args.AbandonMessageAsync(args.Message, cancellationToken: args.CancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bulk upload job {JobId} processing failed.", job.Id);

            // ProcessAsync normally persists Failed before throwing. If persistence
            // failed, leave the broker message available for redelivery/recovery.
            var terminalStatePersisted = false;
            try
            {
                var current = await repository.GetByIdAsync(job.Id, args.CancellationToken);
                terminalStatePersisted = current?.Status is
                    BulkUploadStatus.Failed or
                    BulkUploadStatus.Completed or
                    BulkUploadStatus.CompletedWithErrors;
            }
            catch (Exception stateException)
            {
                _logger.LogError(
                    stateException,
                    "Could not verify persisted state for bulk upload job {JobId}; message will be abandoned.",
                    job.Id);
            }

            if (terminalStatePersisted)
            {
                await args.CompleteMessageAsync(args.Message, args.CancellationToken);
            }
            else
            {
                await args.AbandonMessageAsync(args.Message, cancellationToken: args.CancellationToken);
            }
        }
        finally
        {
            await heartbeatCts.CancelAsync();
            try
            {
                await heartbeat;
            }
            catch (OperationCanceledException) when (heartbeatCts.IsCancellationRequested)
            {
                // Expected when processing finishes before the next heartbeat.
            }
        }
    }

    private async Task RenewLeaseLoopAsync(
        int jobId,
        string claimToken,
        CancellationToken heartbeatToken,
        CancellationTokenSource processingCts)
    {
        while (true)
        {
            await Task.Delay(HeartbeatInterval, heartbeatToken);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IBulkUploadRepository>();
                var renewed = await repository.TryRenewLeaseAsync(
                    jobId,
                    claimToken,
                    JobLeaseDuration,
                    heartbeatToken);

                if (renewed)
                {
                    continue;
                }

                _logger.LogWarning(
                    "Lease renewal returned false for bulk upload job {JobId}; stopping processing.",
                    jobId);
            }
            catch (OperationCanceledException) when (heartbeatToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Lease renewal failed for bulk upload job {JobId}; stopping processing to avoid stale writes.",
                    jobId);
            }

            await processingCts.CancelAsync();
            return;
        }
    }

    private Task HandleErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(
            args.Exception,
            "Bulk upload Service Bus processor error. Entity: {EntityPath}.",
            args.EntityPath);
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
        _processor.ProcessMessageAsync -= HandleMessageAsync;
        _processor.ProcessErrorAsync -= HandleErrorAsync;
        _processor.DisposeAsync().AsTask().GetAwaiter().GetResult();
        base.Dispose();
    }
}
