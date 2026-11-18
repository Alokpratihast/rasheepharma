using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Infrastructure.Configurations;

namespace RashePharma.Infrastructure.Workers;

public sealed class BulkUploadServiceBusConsumer : BackgroundService
{
    private readonly ServiceBusProcessor _processor;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BulkUploadServiceBusConsumer> _logger;

    public BulkUploadServiceBusConsumer(ServiceBusClient client, IOptions<ServiceBusOptions> options,
        IServiceScopeFactory scopeFactory, ILogger<BulkUploadServiceBusConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _processor = client.CreateProcessor(options.Value.BulkUploadQueueName,
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
        try { await Task.Delay(Timeout.Infinite, stoppingToken); }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
        finally { await _processor.StopProcessingAsync(CancellationToken.None); }
    }

    private async Task HandleMessageAsync(ProcessMessageEventArgs args)
    {
        if (!args.Message.ApplicationProperties.TryGetValue("BulkUploadJobId", out var rawId) ||
            !int.TryParse(Convert.ToString(rawId), out var jobId) || jobId <= 0)
        {
            await args.DeadLetterMessageAsync(args.Message, "InvalidBulkUploadJobId",
                "The message does not contain a valid BulkUploadJobId.", args.CancellationToken);
            return;
        }

        await using var scope = _scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBulkUploadRepository>();
        var service = scope.ServiceProvider.GetRequiredService<IBulkUploadService>();
        var job = await repository.ClaimJobAsync(jobId, args.CancellationToken);

        // Duplicate delivery or a job already owned by another worker is safe to acknowledge.
        if (job is null)
        {
            await args.CompleteMessageAsync(args.Message, args.CancellationToken);
            return;
        }

        if (string.IsNullOrWhiteSpace(job.ClaimToken))
            throw new InvalidOperationException($"Claim token missing for bulk upload job {jobId}.");

        using var processingCts = CancellationTokenSource.CreateLinkedTokenSource(args.CancellationToken);
        using var heartbeatCts = CancellationTokenSource.CreateLinkedTokenSource(args.CancellationToken);
        var heartbeat = RenewLeaseLoopAsync(job.Id, job.ClaimToken, heartbeatCts.Token, processingCts);
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
            _logger.LogWarning("Bulk upload job {JobId} processing stopped after lease loss.", job.Id);
            await args.CompleteMessageAsync(args.Message, args.CancellationToken);
        }
        catch (Exception ex)
        {
            // ProcessAsync persists the terminal Failed state when it owns the lease.
            _logger.LogError(ex, "Bulk upload job {JobId} failed.", job.Id);
            await args.CompleteMessageAsync(args.Message, args.CancellationToken);
        }
        finally
        {
            await heartbeatCts.CancelAsync();
            try { await heartbeat; }
            catch (OperationCanceledException) when (heartbeatCts.IsCancellationRequested) { }
        }
    }

    private async Task RenewLeaseLoopAsync(int jobId, string claimToken, CancellationToken heartbeatToken,
        CancellationTokenSource processingCts)
    {
        while (true)
        {
            await Task.Delay(TimeSpan.FromSeconds(30), heartbeatToken);
            using var scope = _scopeFactory.CreateScope();
            var repository = scope.ServiceProvider.GetRequiredService<IBulkUploadRepository>();
            var renewed = await repository.TryRenewLeaseAsync(jobId, claimToken, TimeSpan.FromMinutes(2), heartbeatToken);
            if (renewed) continue;
            _logger.LogWarning("Lease renewal failed for bulk upload job {JobId}.", jobId);
            await processingCts.CancelAsync();
            return;
        }
    }

    private Task HandleErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Bulk upload Service Bus processor error. Entity: {EntityPath}.",
            args.EntityPath);
        return Task.CompletedTask;
    }

    public override async Task DisposeAsync()
    {
        _processor.ProcessMessageAsync -= HandleMessageAsync;
        _processor.ProcessErrorAsync -= HandleErrorAsync;
        await _processor.DisposeAsync();
        await base.DisposeAsync();
    }
}