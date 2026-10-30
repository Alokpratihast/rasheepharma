
using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Domain.Enums.Email;
using RashePharma.Infrastructure.Configurations;

namespace RashePharma.Infrastructure.Services;

public sealed class AzureServiceBusEmailPublisher : IEmailMessagePublisher
{
    private readonly ServiceBusSender _transactionalSender;
    private readonly ServiceBusSender _bulkSender;

    public AzureServiceBusEmailPublisher(
        ServiceBusClient serviceBusClient,
        IOptions<ServiceBusOptions> options)
    {
        ArgumentNullException.ThrowIfNull(serviceBusClient);
        ArgumentNullException.ThrowIfNull(options);

        var settings = options.Value;

        if (string.IsNullOrWhiteSpace(settings.TransactionalQueueName))
            throw new InvalidOperationException(
                "Transactional queue name is required.");

        if (string.IsNullOrWhiteSpace(settings.BulkQueueName))
            throw new InvalidOperationException(
                "Bulk queue name is required.");

        _transactionalSender =
            serviceBusClient.CreateSender(settings.TransactionalQueueName);

        _bulkSender =
            serviceBusClient.CreateSender(settings.BulkQueueName);
    }

    public async Task PublishAsync(
        EmailCategory category,
        Guid outboxMessageId,
        Guid emailJobId,
        string eventType,
        int schemaVersion,
        string payloadJson,
        CancellationToken cancellationToken = default)
    {
        if (outboxMessageId == Guid.Empty)
            throw new ArgumentException(
                "Outbox message ID is required.",
                nameof(outboxMessageId));

        if (emailJobId == Guid.Empty)
            throw new ArgumentException(
                "Email job ID is required.",
                nameof(emailJobId));

        if (string.IsNullOrWhiteSpace(eventType))
            throw new ArgumentException(
                "Event type is required.",
                nameof(eventType));

        if (schemaVersion <= 0)
            throw new ArgumentOutOfRangeException(nameof(schemaVersion));

        if (string.IsNullOrWhiteSpace(payloadJson))
            throw new ArgumentException(
                "Payload JSON is required.",
                nameof(payloadJson));

        cancellationToken.ThrowIfCancellationRequested();

        // Validate that payload is valid JSON.
        using var document = JsonDocument.Parse(payloadJson);

        var queueCategory = category switch
        {
            EmailCategory.Transactional => "Transactional",
            EmailCategory.Bulk => "Bulk",
            _ => throw new ArgumentOutOfRangeException(nameof(category))
        };

        var sender = category switch
        {
            EmailCategory.Transactional => _transactionalSender,
            EmailCategory.Bulk => _bulkSender,
            _ => throw new ArgumentOutOfRangeException(nameof(category))
        };

        var message = new ServiceBusMessage(
            new BinaryData(Encoding.UTF8.GetBytes(payloadJson)))
        {
            MessageId = outboxMessageId.ToString("N"),
            CorrelationId = emailJobId.ToString("N"),
            ContentType = "application/json",
            Subject = eventType
        };

        message.ApplicationProperties["EmailJobId"] =
            emailJobId.ToString("N");

        message.ApplicationProperties["OutboxMessageId"] =
            outboxMessageId.ToString("N");

        message.ApplicationProperties["EventType"] = eventType;

        message.ApplicationProperties["SchemaVersion"] = schemaVersion;

        message.ApplicationProperties["Category"] = queueCategory;

        await sender.SendMessageAsync(message, cancellationToken);
    }
}
