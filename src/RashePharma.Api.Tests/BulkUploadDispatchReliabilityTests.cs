using Xunit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RashePharma.Infrastructure.Services;
using RashePharma.Infrastructure.Workers;

namespace RashePharma.Api.Tests;

public sealed class BulkUploadDispatchReliabilityTests
{
    [Fact]
    public void WorkerServiceRegistration_ValidatesAllConstructorDependencies()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "Server=localhost;Database=RasheePharmaTests;User Id=test;Password=not-used;TrustServerCertificate=True",
                ["ConnectionStrings:AzureStorage"] = "UseDevelopmentStorage=true",
                ["AzureServiceBus:FullyQualifiedNamespace"] = "test.servicebus.windows.net",
                ["AzureServiceBus:BulkUploadQueueName"] = "bulk-upload"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddBulkUploadWorkerServices(configuration);

        using var provider = services.BuildServiceProvider(new ServiceProviderOptions
        {
            ValidateOnBuild = true,
            ValidateScopes = true
        });

        Assert.NotNull(provider);
    }

    [Fact]
    public void RetryWait_UsesScheduledRetryInsteadOfLongRecoveryInterval()
    {
        var now = new DateTime(2026, 11, 18, 12, 0, 0, DateTimeKind.Utc);
        var retryAt = now.AddSeconds(15);

        var wait = BulkUploadDispatchOutboxDispatcher.CalculateWaitDuration(
            TimeSpan.FromSeconds(300),
            retryAt,
            now);

        Assert.Equal(TimeSpan.FromSeconds(15), wait);
    }

    [Fact]
    public void RetryWait_IsBoundedByRecoveryInterval()
    {
        var now = new DateTime(2026, 11, 18, 12, 0, 0, DateTimeKind.Utc);
        var retryAt = now.AddMinutes(10);

        var wait = BulkUploadDispatchOutboxDispatcher.CalculateWaitDuration(
            TimeSpan.FromSeconds(300),
            retryAt,
            now);

        Assert.Equal(TimeSpan.FromSeconds(300), wait);
    }

    [Fact]
    public void RetryWait_ReturnsImmediatelyWhenRetryIsAlreadyDue()
    {
        var now = new DateTime(2026, 11, 18, 12, 0, 0, DateTimeKind.Utc);
        var retryAt = now.AddSeconds(-1);

        var wait = BulkUploadDispatchOutboxDispatcher.CalculateWaitDuration(
            TimeSpan.FromSeconds(300),
            retryAt,
            now);

        Assert.Equal(TimeSpan.Zero, wait);
    }
}
