using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Domain.Enums;
using RashePharma.Infrastructure.Configurations;
using RashePharma.Infrastructure.Services;

namespace RashePharma.Infrastructure.Workers;

public sealed class BulkUploadRabbitMqConsumer : BackgroundService
{
    private static readonly TimeSpan JobLeaseDuration = TimeSpan.FromMinutes(2);
    private static readonly TimeSpan HeartbeatInterval = TimeSpan.FromSeconds(30);

    private readonly RabbitMqOptions _options;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BulkUploadRabbitMqConsumer> _logger;

    public BulkUploadRabbitMqConsumer(
        IOptions<RabbitMqOptions> options,
        IServiceScopeFactory scopeFactory,
        ILogger<BulkUploadRabbitMqConsumer> logger)
    {
        _options = options.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var connection = await RabbitMqBulkUploadPublisher
            .CreateFactory(_options)
            .CreateConnectionAsync(stoppingToken);
        await using (connection)
        await using (var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken))
        {
            await RabbitMqTopology.DeclareAsync(channel, _options, stoppingToken);
            await channel.BasicQosAsync(0, 1, global: false, cancellationToken: stoppingToken);

            var consumer = new AsyncEventingBasicConsumer(channel);
            consumer.ReceivedAsync += (_, args) => HandleMessageAsync(args, channel, stoppingToken);
            var consumerTag = await channel.BasicConsumeAsync(
                queue: _options.BulkUploadQueueName,
                autoAck: false,
                consumer: consumer,
                cancellationToken: stoppingToken);

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
                if (channel.IsOpen)
                    await channel.BasicCancelAsync(consumerTag, cancellationToken: CancellationToken.None);
            }
        }
    }

    private async Task HandleMessageAsync(
        BasicDeliverEventArgs args,
        IChannel channel,
        CancellationToken stoppingToken)
    {
        if (!int.TryParse(args.BasicProperties.CorrelationId, out var jobId) || jobId <= 0)
        {
            await channel.BasicNackAsync(args.DeliveryTag, multiple: false, requeue: false,
                cancellationToken: stoppingToken);
            return;
        }

        await using var scope = _scopeFactory.CreateAsyncScope();
        var repository = scope.ServiceProvider.GetRequiredService<IBulkUploadRepository>();
        var service = scope.ServiceProvider.GetRequiredService<IBulkUploadService>();
        var job = await repository.ClaimJobAsync(jobId, stoppingToken);
        if (job is null)
        {
            var current = await repository.GetByIdAsync(jobId, stoppingToken);
            if (current is null)
            {
                _logger.LogWarning("Bulk upload message references missing job {JobId}.", jobId);
                await channel.BasicNackAsync(args.DeliveryTag, false, false, stoppingToken);
                return;
            }

            if (current.Status == BulkUploadStatus.Pending)
            {
                _logger.LogWarning("Pending job {JobId} could not be claimed; sending message to DLQ for outbox recovery.", jobId);
                await channel.BasicNackAsync(args.DeliveryTag, false, false, stoppingToken);
                return;
            }

            await channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
            return;
        }

        if (string.IsNullOrWhiteSpace(job.ClaimToken))
            throw new InvalidOperationException($"Claim token missing for bulk upload job {jobId}.");

        using var processingCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        using var heartbeatCts = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        var heartbeat = RenewLeaseLoopAsync(job.Id, job.ClaimToken, heartbeatCts.Token, processingCts);

        try
        {
            await service.ProcessAsync(job.Id, job.ClaimToken, processingCts.Token);
            await channel.BasicAckAsync(args.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            throw;
        }
        catch (OperationCanceledException) when (processingCts.IsCancellationRequested)
        {
            _logger.LogWarning("Bulk upload job {JobId} lost its lease; dead-lettering this delivery for SQL outbox recovery.", job.Id);
            await channel.BasicNackAsync(args.DeliveryTag, false, false, stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Bulk upload job {JobId} processing failed.", job.Id);
            var terminalStatePersisted = false;
            try
            {
                await using var verificationScope = _scopeFactory.CreateAsyncScope();
                var verificationRepository = verificationScope.ServiceProvider.GetRequiredService<IBulkUploadRepository>();
                var current = await verificationRepository.GetByIdAsync(job.Id, stoppingToken);
                terminalStatePersisted = current?.Status is
                    BulkUploadStatus.Failed or
                    BulkUploadStatus.Completed or
                    BulkUploadStatus.CompletedWithErrors;
            }
            catch (Exception stateException)
            {
                _logger.LogError(stateException, "Could not verify persisted state for bulk upload job {JobId}.", job.Id);
            }

            if (terminalStatePersisted)
                await channel.BasicAckAsync(args.DeliveryTag, false, stoppingToken);
            else
                // The SQL outbox re-publishes after the processing lease expires. Avoid a hot requeue loop.
                await channel.BasicNackAsync(args.DeliveryTag, false, false, stoppingToken);
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
                // Expected when processing ends before the next heartbeat.
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
                if (await repository.TryRenewLeaseAsync(jobId, claimToken, JobLeaseDuration, heartbeatToken))
                    continue;
                _logger.LogWarning("Lease renewal returned false for bulk upload job {JobId}.", jobId);
            }
            catch (OperationCanceledException) when (heartbeatToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lease renewal failed for bulk upload job {JobId}.", jobId);
            }

            await processingCts.CancelAsync();
            return;
        }
    }
}
