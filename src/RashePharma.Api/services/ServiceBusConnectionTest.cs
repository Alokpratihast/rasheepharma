using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using RashePharma.Infrastructure.Configurations;

namespace RashePharma.Api.Services;

public sealed class ServiceBusConnectionTest
{
    private readonly ServiceBusClient _client;
    private readonly ServiceBusOptions _options;

    public ServiceBusConnectionTest(
        ServiceBusClient client,
        IOptions<ServiceBusOptions> options)
    {
        _client = client;
        _options = options.Value;
    }

    public async Task TestConnectionAsync(
        CancellationToken cancellationToken = default)
    {
        await using var transactionalSender =
            _client.CreateSender(_options.TransactionalQueueName);

        await using var bulkSender =
            _client.CreateSender(_options.BulkQueueName);

        await transactionalSender.CreateMessageBatchAsync(
            cancellationToken);

        await bulkSender.CreateMessageBatchAsync(
            cancellationToken);
    }
}