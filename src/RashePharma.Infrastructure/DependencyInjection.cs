using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Infrastructure.Configurations;
using RashePharma.Infrastructure.Services;

namespace RashePharma.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRabbitMqTransport(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
            .Validate(options =>
                    !string.IsNullOrWhiteSpace(options.Host) &&
                    options.Port is > 0 and <= 65535 &&
                    !string.IsNullOrWhiteSpace(options.VirtualHost) &&
                    !string.IsNullOrWhiteSpace(options.Username) &&
                    !string.IsNullOrWhiteSpace(options.Password) &&
                    !string.IsNullOrWhiteSpace(options.BulkUploadQueueName) &&
                    !string.IsNullOrWhiteSpace(options.DeadLetterExchangeName) &&
                    !string.IsNullOrWhiteSpace(options.DeadLetterQueueName),
                "RabbitMQ connection and queue settings are required.")
            .ValidateOnStart();

        services.AddSingleton<RabbitMqBulkUploadPublisher>();
        services.AddSingleton<IBulkUploadDispatchPublisher>(sp =>
            sp.GetRequiredService<RabbitMqBulkUploadPublisher>());
        return services;
    }

    // API: the SQL transactional outbox remains the durable source of work.
    public static IServiceCollection AddBulkUploadDispatchers(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<BulkUploadDispatchOptions>()
            .Bind(configuration.GetSection(BulkUploadDispatchOptions.SectionName))
            .Validate(options => options.BatchSize > 0 && options.LeaseDurationSeconds > 0 && options.MaxAttempts > 0 &&
                    options.RetryBaseDelaySeconds > 0 && options.RetryMaxDelaySeconds >= options.RetryBaseDelaySeconds &&
                    options.RecoveryIntervalSeconds > 0,
                "Bulk upload dispatch options must be positive.")
            .ValidateOnStart();
        services.AddHostedService<BulkUploadDispatchOutboxDispatcher>();
        return services;
    }
}
