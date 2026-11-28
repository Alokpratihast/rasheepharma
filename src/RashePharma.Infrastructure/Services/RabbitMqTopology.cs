using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RashePharma.Infrastructure.Configurations;

namespace RashePharma.Infrastructure.Services;

internal static class RabbitMqTopology
{
    public static async Task DeclareAsync(
        IChannel channel,
        RabbitMqOptions options,
        CancellationToken cancellationToken)
    {
        await channel.ExchangeDeclareAsync(
            exchange: options.DeadLetterExchangeName,
            type: ExchangeType.Direct,
            durable: true,
            autoDelete: false,
            cancellationToken: cancellationToken);
        await channel.QueueDeclareAsync(
            queue: options.DeadLetterQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            cancellationToken: cancellationToken);
        await channel.QueueBindAsync(
            queue: options.DeadLetterQueueName,
            exchange: options.DeadLetterExchangeName,
            routingKey: options.DeadLetterQueueName,
            cancellationToken: cancellationToken);

        var arguments = new Dictionary<string, object?>
        {
            ["x-dead-letter-exchange"] = options.DeadLetterExchangeName,
            ["x-dead-letter-routing-key"] = options.DeadLetterQueueName
        };
        await channel.QueueDeclareAsync(
            queue: options.BulkUploadQueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: arguments,
            cancellationToken: cancellationToken);
    }
}
