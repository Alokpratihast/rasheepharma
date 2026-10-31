
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Domain.Entities.Email;
using RashePharma.Infrastructure.Configurations;

namespace RashePharma.Infrastructure.Services;

public sealed class OutboxDispatcher : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OutboxDispatcher> _logger;
    private readonly OutboxDispatcherOptions _options;

    public OutboxDispatcher(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxDispatcher> logger,
        IOptions<OutboxDispatcherOptions> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;

        if (_options.BatchSize <= 0 ||
            _options.PollingIntervalSeconds <= 0 ||
            _options.LeaseDurationSeconds <= 0 ||
            _options.MaxAttempts <= 0 ||
            _options.RetryBaseDelaySeconds <= 0 ||
            _options.RetryMaxDelaySeconds < _options.RetryBaseDelaySeconds)
        {
            throw new InvalidOperationException(
                "Invalid OutboxDispatcher configuration.");
        }
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Outbox Dispatcher started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var processedCount =
                    await ProcessBatchAsync(stoppingToken);

                // Immediately process another batch if the current
                // batch was full.
                if (processedCount >= _options.BatchSize)
                    continue;

                await Task.Delay(
                    TimeSpan.FromSeconds(
                        _options.PollingIntervalSeconds),
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Outbox Dispatcher encountered an error.");

                await Task.Delay(
                    TimeSpan.FromSeconds(
                        _options.PollingIntervalSeconds),
                    stoppingToken);
            }
        }

        _logger.LogInformation(
            "Outbox Dispatcher stopped.");
    }

    private async Task<int> ProcessBatchAsync(
        CancellationToken cancellationToken)
    {
        await using var scope =
            _scopeFactory.CreateAsyncScope();

        var outboxRepository = scope.ServiceProvider
            .GetRequiredService<IEmailOutboxRepository>();

        var emailJobRepository = scope.ServiceProvider
            .GetRequiredService<IEmailJobRepository>();

        var unitOfWork = scope.ServiceProvider
            .GetRequiredService<IUnitOfWork>();

        var publisher = scope.ServiceProvider
            .GetRequiredService<IEmailMessagePublisher>();

        var nowUtc = DateTime.UtcNow;

        // ClaimPendingAsync already manages and commits
        // its own SQL transaction.
        var messages =
            await outboxRepository.ClaimPendingAsync(
                _options.BatchSize,
                nowUtc,
                TimeSpan.FromSeconds(
                    _options.LeaseDurationSeconds),
                cancellationToken);

        if (messages.Count == 0)
            return 0;

        _logger.LogInformation(
            "Claimed {MessageCount} outbox messages.",
            messages.Count);

        foreach (var message in messages)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var emailJob =
                await emailJobRepository.GetByIdAsync(
                    message.EmailJobId);

            if (emailJob is null)
            {
                MarkDeadLettered(
                    message,
                    "Associated EmailJob was not found.");

                await unitOfWork.SaveChangesAsync();

                _logger.LogError(
                    "Outbox message {OutboxMessageId} has no associated EmailJob.",
                    message.Id);

                continue;
            }

            try
            {
                await publisher.PublishAsync(
                    emailJob.Category,
                    message.Id,
                    message.EmailJobId,
                    message.EventType,
                    message.SchemaVersion,
                    message.PayloadJson,
                    cancellationToken);

                message.Status = EmailOutboxStatus.Published;
                message.PublishedAtUtc = DateTime.UtcNow;
                message.LeaseUntilUtc = null;
                message.NextAttemptAtUtc = null;
                message.LastError = null;

                await unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "Outbox message {OutboxMessageId} published successfully.",
                    message.Id);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                // Do not mark an ambiguous publish as failed.
                // The lease will expire and the message can be retried.
                throw;
            }
            catch (Exception exception)
            {
                HandlePublishFailure(message, exception);

                await unitOfWork.SaveChangesAsync();

                _logger.LogWarning(
                    exception,
                    "Publishing outbox message {OutboxMessageId} failed. Status: {Status}, Attempt: {AttemptCount}.",
                    message.Id,
                    message.Status,
                    message.AttemptCount);
            }
        }

        return messages.Count;
    }

    private void HandlePublishFailure(
        EmailOutboxMessage message,
        Exception exception)
    {
        message.LeaseUntilUtc = null;
        message.LastError = TruncateError(exception.Message);

        var isPermanentFailure =
            exception is JsonException ||
            exception is ArgumentException ||
            (exception is ServiceBusException serviceBusException &&
             !serviceBusException.IsTransient);

        if (isPermanentFailure ||
            message.AttemptCount >= _options.MaxAttempts)
        {
            MarkDeadLettered(
                message,
                exception.Message);

            return;
        }

        message.Status = EmailOutboxStatus.RetryScheduled;

        message.NextAttemptAtUtc =
            DateTime.UtcNow.Add(
                CalculateRetryDelay(message.AttemptCount));
    }

    private void MarkDeadLettered(
        EmailOutboxMessage message,
        string error)
    {
        message.Status = EmailOutboxStatus.DeadLettered;
        message.LeaseUntilUtc = null;
        message.NextAttemptAtUtc = null;
        message.LastError = TruncateError(error);
    }

    private TimeSpan CalculateRetryDelay(int attemptCount)
    {
        var exponent = Math.Max(0, attemptCount - 1);

        var exponentialSeconds =
            _options.RetryBaseDelaySeconds *
            Math.Pow(2, exponent);

        var cappedSeconds = Math.Min(
            exponentialSeconds,
            _options.RetryMaxDelaySeconds);

        // Add 80%-120% jitter to reduce synchronized retries.
        var jitterFactor =
            0.8 + (Random.Shared.NextDouble() * 0.4);

        var finalSeconds = Math.Min(
            cappedSeconds * jitterFactor,
            _options.RetryMaxDelaySeconds);

        return TimeSpan.FromSeconds(finalSeconds);
    }

    private static string TruncateError(string error)
    {
        const int maxLength = 2000;

        if (string.IsNullOrEmpty(error))
            return "Unknown publishing error.";

        return error.Length <= maxLength
            ? error
            : error[..maxLength];
    }
}
