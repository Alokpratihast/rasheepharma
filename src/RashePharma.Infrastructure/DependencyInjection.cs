
using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Infrastructure.Configurations;
using RashePharma.Infrastructure.Services;

namespace RashePharma.Infrastructure;

public static class DependencyInjection
{
    // Shared: Azure Service Bus client and publisher
    public static IServiceCollection AddAzureServiceBus(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<ServiceBusOptions>()
            .Bind(configuration.GetSection(ServiceBusOptions.SectionName))
            .Validate(
                options =>
                    !string.IsNullOrWhiteSpace(options.ConnectionString) ||
                    !string.IsNullOrWhiteSpace(options.FullyQualifiedNamespace),
                "Azure Service Bus connection string or namespace is required.")
            .Validate(options => !string.IsNullOrWhiteSpace(options.BulkUploadQueueName), "Bulk upload queue name is required.")
            .ValidateOnStart();

        services.AddSingleton<ServiceBusClient>(serviceProvider =>
        {
            var options = serviceProvider
                .GetRequiredService<IOptions<ServiceBusOptions>>()
                .Value;

            if (!string.IsNullOrWhiteSpace(options.ConnectionString))
            {
                return new ServiceBusClient(options.ConnectionString);
            }

            return new ServiceBusClient(
                options.FullyQualifiedNamespace,
                new DefaultAzureCredential());
        });

        services.AddSingleton<AzureServiceBusBulkUploadPublisher>();

        return services;
    }

    // API: Bulk-upload dispatch only; transactional emails are sent directly by the API.
    public static IServiceCollection AddBulkUploadDispatchers(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<BulkUploadDispatchOptions>()
            .Bind(configuration.GetSection(BulkUploadDispatchOptions.SectionName))
            .Validate(options => options.BatchSize > 0 && options.LeaseDurationSeconds > 0 && options.MaxAttempts > 0,
                "Bulk upload dispatch options must be positive.")
            .ValidateOnStart();
        services.AddHostedService<BulkUploadDispatchOutboxDispatcher>();

        return services;
    }

}
