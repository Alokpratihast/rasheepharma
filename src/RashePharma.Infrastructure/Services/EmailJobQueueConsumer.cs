using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RashePharma.Infrastructure.Configurations;

namespace RashePharma.Infrastructure.Services;

public sealed class EmailJobQueueConsumer : BackgroundService
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusOptions _queueOptions;
    private readonly EmailJobConsumerOptions _consumerOptions;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailJobQueueConsumer> _logger;
    private readonly List<ServiceBusProcessor> _processors = [];

    public EmailJobQueueConsumer(
        ServiceBusClient client,
        IOptions<ServiceBusOptions> queueOptions,
        IOptions<EmailJobConsumerOptions> consumerOptions,
        IServiceScopeFactory scopeFactory,
        ILogger<EmailJobQueueConsumer> logger)
    {
        _client = client;
        _queueOptions = queueOptions.Value;
        _consumerOptions = consumerOptions.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;

        if (_consumerOptions.MaxConcurrentCalls <= 0 || _consumerOptions.ProcessingLeaseMinutes <= 0)
            throw new InvalidOperationException("EmailJobConsumer configuration is invalid.");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var processorOptions = new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = _consumerOptions.MaxConcurrentCalls,
            PrefetchCount = 0,
            MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(_consumerOptions.ProcessingLeaseMinutes)
        };

        foreach (var queueName in new[] { _queueOptions.TransactionalQueueName, _queueOptions.BulkQueueName }.Distinct(StringComparer.Ordinal))
        {
            var processor = _client.CreateProcessor(queueName, processorOptions);
            processor.ProcessMessageAsync += ProcessMessageAsync;
            processor.ProcessErrorAsync += ProcessErrorAsync;
            _processors.Add(processor);
            await processor.StartProcessingAsync(stoppingToken);
            _logger.LogInformation("Email job consumer started for queue {QueueName}.", queueName);
        }

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
        }
        finally
        {
            foreach (var processor in _processors)
                await processor.StopProcessingAsync(CancellationToken.None);
        }
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        if (!TryReadEmailJobId(args.Message, out var emailJobId))
        {
            await args.DeadLetterMessageAsync(args.Message, "InvalidEmailJobMessage", "EmailJobId is missing or invalid.", args.CancellationToken);
            return;
        }

        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var handler = scope.ServiceProvider.GetRequiredService<EmailJobMessageHandler>();
            var result = await handler.HandleAsync(emailJobId, args.CancellationToken);
            switch (result)
            {
                case EmailJobMessageResult.Completed:
                    await args.CompleteMessageAsync(args.Message, args.CancellationToken);
                    break;
                case EmailJobMessageResult.NotFound:
                case EmailJobMessageResult.Invalid:
                    await args.DeadLetterMessageAsync(args.Message, "InvalidEmailJob", "The referenced email job could not be processed.", args.CancellationToken);
                    break;
                case EmailJobMessageResult.Busy:
                    await args.AbandonMessageAsync(args.Message, cancellationToken: args.CancellationToken);
                    break;
            }
        }
        catch (OperationCanceledException) when (args.CancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Email job message {EmailJobId} failed; Service Bus will redeliver it.", emailJobId);
            await args.AbandonMessageAsync(args.Message, cancellationToken: args.CancellationToken);
        }
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(args.Exception, "Service Bus email processor error. Entity={EntityPath}, Source={ErrorSource}.", args.EntityPath, args.ErrorSource);
        return Task.CompletedTask;
    }

    private static bool TryReadEmailJobId(ServiceBusReceivedMessage message, out Guid id)
    {
        if (message.ApplicationProperties.TryGetValue("EmailJobId", out var propertyValue) &&
            Guid.TryParse(propertyValue?.ToString(), out id))
            return true;

        try
        {
            using var payload = JsonDocument.Parse(message.Body);
            if (payload.RootElement.TryGetProperty("EmailJobId", out var idValue) &&
                Guid.TryParse(idValue.ToString(), out id))
                return true;
        }
        catch (JsonException)
        {
        }

        id = Guid.Empty;
        return false;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var processor in _processors)
        {
            await processor.StopProcessingAsync(cancellationToken);
            await processor.DisposeAsync();
        }
        await base.StopAsync(cancellationToken);
    }
}
