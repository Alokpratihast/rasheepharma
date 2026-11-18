using System.Text;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using RashePharma.Domain.Entities.BulkUpload;
using RashePharma.Infrastructure.Configurations;

namespace RashePharma.Infrastructure.Services;

public sealed class AzureServiceBusBulkUploadPublisher : IAsyncDisposable
{
    private readonly ServiceBusSender _sender;
    public AzureServiceBusBulkUploadPublisher(ServiceBusClient client, IOptions<ServiceBusOptions> options)
    {
        _sender = client.CreateSender(options.Value.BulkUploadQueueName);
    }

    public async Task PublishAsync(BulkUploadDispatchOutbox message, CancellationToken cancellationToken)
    {
        using var document = JsonDocument.Parse(message.PayloadJson);
        var sbMessage = new ServiceBusMessage(Encoding.UTF8.GetBytes(message.PayloadJson))
        {
            MessageId = message.Id.ToString("N"),
            CorrelationId = message.BulkUploadJobId.ToString(),
            ContentType = "application/json",
            Subject = message.EventType
        };
        sbMessage.ApplicationProperties["BulkUploadJobId"] = message.BulkUploadJobId;
        sbMessage.ApplicationProperties["OutboxMessageId"] = message.Id.ToString("N");
        sbMessage.ApplicationProperties["SchemaVersion"] = message.SchemaVersion;
        await _sender.SendMessageAsync(sbMessage, cancellationToken);
    }

    public ValueTask DisposeAsync() => _sender.DisposeAsync();
}
