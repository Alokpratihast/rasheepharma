
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
    private readonly IEmailOutboxSignal _outboxSignal;

    public OutboxDispatcher(
        IServiceScopeFactory scopeFactory,
        ILogger<OutboxDispatcher> logger,
        IOptions<OutboxDispatcherOptions> options,
        IEmailOutboxSignal outboxSignal)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
        _outboxSignal = outboxSignal;

        if (_options.BatchSize <= 0 ||
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
                var result = await ProcessBatchAsync(stoppingToken);

                // Continue immediately when the batch may have more eligible rows.
                if (result.Count >= _options.BatchSize)
                    continue;

                // A new outbox commit wakes this dispatcher immediately. When no
                // retry/lease is due, it sleeps indefinitely and performs no SQL polling.
                var wait = result.NextRetryAtUtc is null
                    ? Timeout.InfiniteTimeSpan
                    : TimeSpan.FromSeconds(Math.Max(
                        0,
                        (result.NextRetryAtUtc.Value - DateTime.UtcNow).TotalSeconds));

                await _outboxSignal.WaitForMessageAsync(wait, stoppingToken);
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

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }

        _logger.LogInformation(
            "Outbox Dispatcher stopped.");
    }

    private async Task<(int Count, DateTime? NextRetryAtUtc)> ProcessBatchAsync(
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
        {
            var nextRetry = await outboxRepository.GetNextRetryAtUtcAsync(cancellationToken);
            return (0, nextRetry);
        }

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

        var nextRetryAtUtc = await outboxRepository.GetNextRetryAtUtcAsync(cancellationToken);
        return (messages.Count, nextRetryAtUtc);
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
