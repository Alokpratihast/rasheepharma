
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
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.TransactionalQueueName),
                "Transactional queue name is required.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.BulkQueueName),
                "Bulk queue name is required.")
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

        services.AddSingleton<
            IEmailMessagePublisher,
            AzureServiceBusEmailPublisher>();

        return services;
    }

    // API: Only Outbox Dispatcher
    public static IServiceCollection AddOutboxDispatcher(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<OutboxDispatcherOptions>()
            .Bind(configuration.GetSection(OutboxDispatcherOptions.SectionName))
            .Validate(
                options => options.BatchSize > 0,
                "Outbox batch size must be greater than zero.")
            .Validate(
                options => options.PollingIntervalSeconds > 0,
                "Polling interval must be greater than zero.")
            .Validate(
                options => options.LeaseDurationSeconds > 0,
                "Lease duration must be greater than zero.")
            .Validate(
                options => options.MaxAttempts > 0,
                "Maximum attempts must be greater than zero.")
            .Validate(
                options =>
                    options.RetryBaseDelaySeconds > 0 &&
                    options.RetryMaxDelaySeconds >= options.RetryBaseDelaySeconds,
                "Retry delay configuration is invalid.")
            .ValidateOnStart();

        services.AddHostedService<OutboxDispatcher>();

        return services;
    }

    // EmailWorker: Only Service Bus Consumer
    public static IServiceCollection AddEmailJobConsumer(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddOptions<EmailJobConsumerOptions>()
            .Bind(configuration.GetSection(EmailJobConsumerOptions.SectionName))
            .Validate(
                options => options.MaxConcurrentCalls > 0,
                "Email consumer concurrency must be greater than zero.")
            .Validate(
                options => options.ProcessingLeaseMinutes > 0,
                "Email processing lease must be greater than zero.")
            .Validate(options => options.ServiceBusLockRenewalMinutes > 0,
                "Service Bus lock renewal duration must be greater than zero.")
            .ValidateOnStart();

        services
            .AddOptions<EmailQuotaOptions>()
            .Bind(configuration.GetSection(EmailQuotaOptions.SectionName))
            .Validate(options => options.OutreachDailyLimit > 0,
                "Outreach daily quota must be greater than zero.")
            .ValidateOnStart();
        services.AddScoped<IEmailOutreachQuotaService, EmailOutreachQuotaService>();
        services.AddScoped<IEmailJobSender, EmailService>();
        services.AddScoped<EmailJobMessageHandler>();
        services.AddHostedService<EmailJobQueueConsumer>();

        return services;
    }
}
