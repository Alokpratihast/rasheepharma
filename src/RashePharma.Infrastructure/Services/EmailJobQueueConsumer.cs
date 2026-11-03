using System.Security.Cryptography;
using System.Text;
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

    // Ensures that the total number of handlers processing jobs
    // across both queues never exceeds MaxConcurrentCalls.
    private readonly SemaphoreSlim _processingSemaphore;

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

        // Validate consumer configuration before starting processors.
        if (_consumerOptions.MaxConcurrentCalls <= 0 ||
            _consumerOptions.ProcessingLeaseMinutes <= 0 ||
            _consumerOptions.ServiceBusLockRenewalMinutes <= 0)
        {
            throw new InvalidOperationException(
                "EmailJobConsumer configuration is invalid. " +
                "Concurrency, processing lease and lock renewal must be greater than zero.");
        }

        // Lock renewal should cover the configured processing lease.
        if (_consumerOptions.ServiceBusLockRenewalMinutes <
            _consumerOptions.ProcessingLeaseMinutes)
        {
            throw new InvalidOperationException(
                "ServiceBusLockRenewalMinutes must be greater than " +
                "or equal to ProcessingLeaseMinutes.");
        }

        // Queue names must be configured.
        if (string.IsNullOrWhiteSpace(_queueOptions.TransactionalQueueName) ||
            string.IsNullOrWhiteSpace(_queueOptions.BulkQueueName))
        {
            throw new InvalidOperationException(
                "Both transactional and bulk queue names must be configured.");
        }

        // One shared semaphore controls actual handler concurrency
        // across transactional and bulk processors.
        _processingSemaphore = new SemaphoreSlim(
            _consumerOptions.MaxConcurrentCalls,
            _consumerOptions.MaxConcurrentCalls);
    }

    /// <summary>
    /// Starts Service Bus processors for the configured queues.
    /// Each queue gets its own processor, while a shared semaphore
    /// controls total concurrent handler execution.
    /// </summary>
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        var queueNames = new[]
        {
            _queueOptions.TransactionalQueueName,
            _queueOptions.BulkQueueName
        }
        .Distinct(StringComparer.Ordinal)
        .ToArray();

        // Divide callback concurrency across processors.
        // The shared semaphore remains the final global limit.
        var processorConcurrency = Math.Max(
            1,
            (int)Math.Ceiling(
                (double)_consumerOptions.MaxConcurrentCalls /
                queueNames.Length));

        var processorOptions = new ServiceBusProcessorOptions
        {
            AutoCompleteMessages = false,
            MaxConcurrentCalls = processorConcurrency,
            PrefetchCount = 0,

            // Maximum period during which the SDK can automatically
            // renew a message lock while its callback is executing.
            MaxAutoLockRenewalDuration =
                TimeSpan.FromMinutes(
                    _consumerOptions.ServiceBusLockRenewalMinutes)
        };

        try
        {
            foreach (var queueName in queueNames)
            {
                stoppingToken.ThrowIfCancellationRequested();

                var processor = _client.CreateProcessor(
                    queueName,
                    processorOptions);

                processor.ProcessMessageAsync += ProcessMessageAsync;
                processor.ProcessErrorAsync += ProcessErrorAsync;

                // Store processor before starting it so that even a
                // partially started processor can be disposed safely.
                _processors.Add(processor);

                await processor.StartProcessingAsync(stoppingToken);

                _logger.LogInformation(
                    "Email job consumer started for queue {QueueName}. " +
                    "Processor concurrency: {Concurrency}",
                    queueName,
                    processorConcurrency);
            }

            // Keep the hosted service alive until application shutdown.
            await Task.Delay(
                Timeout.InfiniteTimeSpan,
                stoppingToken);
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
            // Expected during application shutdown.
            _logger.LogInformation(
                "Email job consumer shutdown requested.");
        }
        finally
        {
            // Stop and dispose every processor during shutdown.
            foreach (var processor in _processors)
            {
                try
                {
                    await processor.StopProcessingAsync(
                        CancellationToken.None);
                }
                catch (Exception exception)
                {
                    _logger.LogWarning(
                        exception,
                        "Error stopping Service Bus processor.");
                }
                finally
                {
                    await processor.DisposeAsync();
                }
            }

            _processors.Clear();

            _logger.LogInformation(
                "All email queue processors have stopped.");
        }
    }

    /// <summary>
    /// Processes one Service Bus message.
    /// Reads EmailJobId, invokes the handler and completes,
    /// dead-letters, schedules or abandons the message
    /// according to the handler result.
    /// </summary>
    private async Task ProcessMessageAsync(
        ProcessMessageEventArgs args)
    {
        Guid emailJobId = Guid.Empty;
        var semaphoreAcquired = false;

        try
        {
            // Reject messages that do not contain a valid EmailJobId.
            if (!TryReadEmailJobId(args.Message, out emailJobId))
            {
                await args.DeadLetterMessageAsync(
                    args.Message,
                    "InvalidEmailJobMessage",
                    "EmailJobId is missing or invalid.",
                    args.CancellationToken);

                _logger.LogWarning(
                    "Dead-lettered invalid email job message. MessageId={MessageId}",
                    args.Message.MessageId);

                return;
            }

            // Wait for a global processing slot.
            // This limits actual handler execution across both queues.
            await _processingSemaphore.WaitAsync(
                args.CancellationToken);

            semaphoreAcquired = true;

            // Create a separate dependency injection scope
            // for every message.
            await using var scope =
                _scopeFactory.CreateAsyncScope();

            var handler = scope.ServiceProvider
                .GetRequiredService<EmailJobMessageHandler>();

            // Execute the email job handler.
            var result = await handler.HandleAsync(
                emailJobId,
                args.CancellationToken);

            switch (result.Result)
            {
                case EmailJobMessageResult.Completed:

                    // Handler finished successfully or confirmed
                    // that this job is already in a terminal state.
                    await args.CompleteMessageAsync(
                        args.Message,
                        args.CancellationToken);

                    _logger.LogInformation(
                        "Email job {EmailJobId} message completed.",
                        emailJobId);

                    break;

                case EmailJobMessageResult.NotFound:
                case EmailJobMessageResult.Invalid:

                    // Invalid or missing database jobs should not
                    // be repeatedly delivered.
                    await args.DeadLetterMessageAsync(
                        args.Message,
                        "InvalidEmailJob",
                        "The referenced email job could not be processed.",
                        args.CancellationToken);

                    _logger.LogWarning(
                        "Email job {EmailJobId} was dead-lettered.",
                        emailJobId);

                    break;

                case EmailJobMessageResult.Busy:

                    // Job is currently leased or not yet eligible.
                    // Schedule another message instead of immediately
                    // increasing the original message delivery count.
                    await ScheduleRetryAsync(
                        args,
                        emailJobId,
                        result.RetryAtUtc);

                    break;

                default:

                    // Unknown handler results are treated as transient
                    // failures. Service Bus may redeliver the message.
                    _logger.LogError(
                        "Unknown handler result {Result} for email job {EmailJobId}.",
                        result.Result,
                        emailJobId);

                    await AbandonSafelyAsync(args);

                    break;
            }
        }
        catch (OperationCanceledException)
            when (args.CancellationToken.IsCancellationRequested)
        {
            // Do not acknowledge messages during cancellation.
            // Service Bus can redeliver them after lock expiry.
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Email job message {EmailJobId} failed. " +
                "Service Bus will attempt redelivery.",
                emailJobId);

            // Abandon the message when processing fails.
            // This helper also handles failures during abandonment.
            await AbandonSafelyAsync(args);
        }
        finally
        {
            // Always release the global concurrency slot.
            if (semaphoreAcquired)
            {
                _processingSemaphore.Release();
            }
        }
    }

    /// <summary>
    /// Schedules a replacement message for a Busy email job.
    /// The retry MessageId is deterministic for the original message
    /// and retry time, helping Service Bus duplicate detection.
    /// </summary>
    private async Task ScheduleRetryAsync(
        ProcessMessageEventArgs args,
        Guid emailJobId,
        DateTime? retryAtUtc)
    {
        // Use the handler's retry time when supplied.
        // Otherwise derive a stable fallback from the original
        // message enqueue time.
        var requestedRetryTime = retryAtUtc.HasValue
            ? NormalizeUtc(retryAtUtc.Value)
            : args.Message.EnqueuedTime.UtcDateTime.AddSeconds(10);

        var now = DateTime.UtcNow;

        // If the requested retry time is already in the past,
        // schedule a short delay from the current time.
        var actualRetryTime = requestedRetryTime <= now
            ? now.AddSeconds(5)
            : requestedRetryTime;

        var retryAt = new DateTimeOffset(
            DateTime.SpecifyKind(
                actualRetryTime,
                DateTimeKind.Utc));

        // Build a deterministic ID using the original message ID,
        // job ID and requested retry timestamp.
        // Repeated attempts to schedule the same retry therefore
        // use the same MessageId within the duplicate detection window.
        var retryMessageId = CreateRetryMessageId(
            args.Message.MessageId,
            emailJobId,
            requestedRetryTime);

        var scheduledMessage = new ServiceBusMessage(
            args.Message.Body)
        {
            MessageId = retryMessageId,
            CorrelationId = args.Message.CorrelationId,
            ContentType = args.Message.ContentType,
            Subject = args.Message.Subject
        };

        // Preserve application properties required by downstream
        // consumers and message tracing.
        foreach (var property in args.Message.ApplicationProperties)
        {
            scheduledMessage.ApplicationProperties[property.Key] =
                property.Value;
        }

        await using var sender = _client.CreateSender(
            args.EntityPath);

        // Schedule the replacement message first.
        // If scheduling fails, the original message is not completed.
        await sender.ScheduleMessageAsync(
            scheduledMessage,
            retryAt,
            args.CancellationToken);

        _logger.LogInformation(
            "Scheduled retry for email job {EmailJobId}. " +
            "RetryAtUtc={RetryAtUtc}, Queue={QueueName}, RetryMessageId={RetryMessageId}",
            emailJobId,
            retryAt,
            args.EntityPath,
            retryMessageId);

        // Complete the original only after scheduling succeeds.
        // These two operations are NOT atomic. If completion fails,
        // the original may be redelivered. Stable MessageId and
        // duplicate detection reduce, but do not eliminate, that risk.
        await args.CompleteMessageAsync(
            args.Message,
            args.CancellationToken);
    }

    /// <summary>
    /// Creates a deterministic retry MessageId.
    /// A SHA-256 hash keeps the resulting ID short and within
    /// Service Bus MessageId length limits.
    /// </summary>
    private static string CreateRetryMessageId(
        string originalMessageId,
        Guid emailJobId,
        DateTime requestedRetryTimeUtc)
    {
        var input =
            $"{originalMessageId}|{emailJobId:N}|{requestedRetryTimeUtc.Ticks}";

        var hash = SHA256.HashData(
            Encoding.UTF8.GetBytes(input));

        // 32 hexadecimal characters are sufficient for a compact ID.
        var shortHash = Convert.ToHexString(hash)[..32];

        return $"retry-{shortHash}";
    }

    /// <summary>
    /// Converts DateTime values to UTC.
    /// Local values are converted; unspecified values are treated
    /// as UTC because the input contract is RetryAtUtc.
    /// </summary>
    private static DateTime NormalizeUtc(DateTime value)
    {
        return value.Kind switch
        {
            DateTimeKind.Utc => value,

            DateTimeKind.Local => value.ToUniversalTime(),

            _ => DateTime.SpecifyKind(
                value,
                DateTimeKind.Utc)
        };
    }

    /// <summary>
    /// Attempts to abandon a message after a transient processing
    /// failure. Logs abandonment errors without hiding the original
    /// processing exception.
    /// </summary>
    private async Task AbandonSafelyAsync(
        ProcessMessageEventArgs args)
    {
        try
        {
            await args.AbandonMessageAsync(
                args.Message,
                cancellationToken: args.CancellationToken);
        }
        catch (OperationCanceledException)
            when (args.CancellationToken.IsCancellationRequested)
        {
            // The lock may be released naturally during shutdown.
            throw;
        }
        catch (Exception abandonException)
        {
            _logger.LogWarning(
                abandonException,
                "Could not abandon message {MessageId}. " +
                "The message may be redelivered after lock expiry.",
                args.Message.MessageId);
        }
    }

    /// <summary>
    /// Handles errors raised by the Azure Service Bus processor
    /// outside the normal message callback.
    /// </summary>
    private Task ProcessErrorAsync(
        ProcessErrorEventArgs args)
    {
        _logger.LogError(
            args.Exception,
            "Service Bus email processor error. " +
            "Entity={EntityPath}, Source={ErrorSource}.",
            args.EntityPath,
            args.ErrorSource);

        return Task.CompletedTask;
    }

    /// <summary>
    /// Extracts EmailJobId from the Service Bus application properties
    /// first, then falls back to the JSON message body.
    ///
    /// Only a JSON object containing a valid GUID string is accepted.
    /// Malformed JSON and non-object JSON structures are rejected.
    /// </summary>
    private static bool TryReadEmailJobId(
        ServiceBusReceivedMessage message,
        out Guid id)
    {
        id = Guid.Empty;

        // Prefer the application property because it avoids
        // parsing the message body.
        if (message.ApplicationProperties.TryGetValue(
                "EmailJobId",
                out var propertyValue) &&
            Guid.TryParse(
                propertyValue?.ToString(),
                out id))
        {
            return true;
        }

        try
        {
            using var payload = JsonDocument.Parse(message.Body);

            // The root must be a JSON object.
            if (payload.RootElement.ValueKind !=
                JsonValueKind.Object)
            {
                return false;
            }

            // EmailJobId must exist and be a JSON string.
            if (!payload.RootElement.TryGetProperty(
                    "EmailJobId",
                    out var idValue) ||
                idValue.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            return Guid.TryParse(
                idValue.GetString(),
                out id);
        }
        catch (JsonException)
        {
            // Invalid JSON is rejected.
            return false;
        }
        catch (InvalidOperationException)
        {
            // Unexpected JSON structure is rejected safely.
            return false;
        }
    }
}