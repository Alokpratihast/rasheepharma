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
 public AzureServiceBusBulkUploadPublisher(ServiceBusClient client,IOptions<ServiceBusOptions> options)=>_sender=client.CreateSender(options.Value.BulkUploadQueueName);
 public async Task PublishAsync(BulkUploadDispatchOutbox item,CancellationToken ct)
 {
  using var _=JsonDocument.Parse(item.PayloadJson);
  var message=new ServiceBusMessage(Encoding.UTF8.GetBytes(item.PayloadJson)){MessageId=item.Id.ToString("N"),CorrelationId=item.BulkUploadJobId.ToString(),ContentType="application/json",Subject=item.EventType};
  message.ApplicationProperties["BulkUploadJobId"]=item.BulkUploadJobId;
  message.ApplicationProperties["OutboxMessageId"]=item.Id.ToString("N");
  message.ApplicationProperties["SchemaVersion"]=item.SchemaVersion;
  await _sender.SendMessageAsync(message,ct);
 }
 public ValueTask DisposeAsync()=>_sender.DisposeAsync();
}