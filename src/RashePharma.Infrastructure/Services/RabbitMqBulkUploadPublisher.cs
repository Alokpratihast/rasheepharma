using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Domain.Entities.BulkUpload;
using RashePharma.Infrastructure.Configurations;

namespace RashePharma.Infrastructure.Services;

public sealed class RabbitMqBulkUploadPublisher : IBulkUploadDispatchPublisher, IAsyncDisposable
{
    private readonly RabbitMqOptions _options;
    private readonly SemaphoreSlim _gate = new(1, 1);
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqBulkUploadPublisher(IOptions<RabbitMqOptions> options) => _options = options.Value;

    public async Task PublishAsync(BulkUploadDispatchOutbox item, CancellationToken cancellationToken)
    {
        using var _ = JsonDocument.Parse(item.PayloadJson);
        await _gate.WaitAsync(cancellationToken);
        try
        {
            var channel = await GetChannelAsync(cancellationToken);
            var properties = new BasicProperties
            {
                MessageId = item.Id.ToString("N"),
                CorrelationId = item.BulkUploadJobId.ToString(),
                ContentType = "application/json",
                Type = item.EventType,
                DeliveryMode = DeliveryModes.Persistent,
                Headers = new Dictionary<string, object?>
                {
                    ["OutboxMessageId"] = item.Id.ToString("N"),
                    ["SchemaVersion"] = item.SchemaVersion
                }
            };

            await channel.BasicPublishAsync(
                exchange: string.Empty,
                routingKey: _options.BulkUploadQueueName,
                mandatory: true,
                basicProperties: properties,
                body: Encoding.UTF8.GetBytes(item.PayloadJson),
                cancellationToken: cancellationToken);
        }
        finally
        {
            _gate.Release();
        }
    }

    private async Task<IChannel> GetChannelAsync(CancellationToken cancellationToken)
    {
        if (_channel?.IsOpen == true) return _channel;
        if (_channel is not null)
        {
            await _channel.DisposeAsync();
            _channel = null;
        }
        if (_connection?.IsOpen != true)
        {
            if (_connection is not null) await _connection.DisposeAsync();
            var factory = CreateFactory();
            _connection = await factory.CreateConnectionAsync(cancellationToken);
        }

        _channel = await _connection.CreateChannelAsync(
            new CreateChannelOptions(
                publisherConfirmationsEnabled: true,
                publisherConfirmationTrackingEnabled: true),
            cancellationToken);
        await RabbitMqTopology.DeclareAsync(_channel, _options, cancellationToken);
        return _channel;
    }

    internal static ConnectionFactory CreateFactory(RabbitMqOptions options) => new()
    {
        HostName = options.Host,
        Port = options.Port,
        VirtualHost = options.VirtualHost,
        UserName = options.Username,
        Password = options.Password,
        AutomaticRecoveryEnabled = true,
        NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
        RequestedHeartbeat = TimeSpan.FromSeconds(30)
    };

    private ConnectionFactory CreateFactory() => CreateFactory(_options);

    public async ValueTask DisposeAsync()
    {
        if (_channel is not null) await _channel.DisposeAsync();
        if (_connection is not null) await _connection.DisposeAsync();
        _gate.Dispose();
    }
}
