using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Infrastructure.Services;
using Xunit;

namespace RashePharma.Api.Tests;

public sealed class EmailNotificationSignalTests
{
    [Fact]
    public async Task WaitForWorkAsync_WhenNotified_ReturnsTrue()
    {
        var signal = new EmailNotificationSignal();
        signal.NotifyWorkAvailable();

        Assert.True(await signal.WaitForWorkAsync(TimeSpan.FromSeconds(1), CancellationToken.None));
    }

    [Fact]
    public async Task NotifyWorkAvailable_MultipleNotificationsAreCoalesced()
    {
        var signal = new EmailNotificationSignal();
        signal.NotifyWorkAvailable();
        signal.NotifyWorkAvailable();

        Assert.True(await signal.WaitForWorkAsync(TimeSpan.FromSeconds(1), CancellationToken.None));
        Assert.False(await signal.WaitForWorkAsync(TimeSpan.FromMilliseconds(50), CancellationToken.None));
    }

    [Fact]
    public async Task WaitForWorkAsync_WhenNoNotification_ReturnsFalseAfterTimeout()
    {
        var signal = new EmailNotificationSignal();

        Assert.False(await signal.WaitForWorkAsync(TimeSpan.FromMilliseconds(50), CancellationToken.None));
    }

    [Fact]
    public async Task WaitForWorkAsync_WhenCallerCancels_ThrowsOperationCanceledException()
    {
        var signal = new EmailNotificationSignal();
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => signal.WaitForWorkAsync(TimeSpan.FromSeconds(1), cancellation.Token));
    }

    [Fact]
    public async Task Worker_WhenSignaled_DrainsAgainWithoutWaitingForRecovery()
    {
        var signal = new EmailNotificationSignal();
        var service = new CountingNotificationService();
        using var provider = BuildProvider(service);
        var configuration = BuildConfiguration(recoveryIntervalSeconds: 3600);
        using var worker = new EmailNotificationWorker(
            provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<EmailNotificationWorker>.Instance,
            configuration,
            signal);

        await worker.StartAsync(CancellationToken.None);
        try
        {
            await service.WaitForCallAsync(TimeSpan.FromSeconds(2));
            signal.NotifyWorkAvailable();
            await service.WaitForCallAsync(TimeSpan.FromSeconds(2));
            Assert.Equal(2, service.CallCount);
        }
        finally
        {
            using var stop = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            await worker.StopAsync(stop.Token);
        }
    }

    [Fact]
    public async Task Worker_WhenSignalIsMissed_RunsPeriodicRecoverySweep()
    {
        var signal = new EmailNotificationSignal();
        var service = new CountingNotificationService();
        using var provider = BuildProvider(service);
        var configuration = BuildConfiguration(recoveryIntervalSeconds: 1);
        using var worker = new EmailNotificationWorker(
            provider.GetRequiredService<IServiceScopeFactory>(),
            NullLogger<EmailNotificationWorker>.Instance,
            configuration,
            signal);

        await worker.StartAsync(CancellationToken.None);
        try
        {
            await service.WaitForCallAsync(TimeSpan.FromSeconds(2));
            await service.WaitForCallAsync(TimeSpan.FromSeconds(4));
            Assert.Equal(2, service.CallCount);
        }
        finally
        {
            using var stop = new CancellationTokenSource(TimeSpan.FromSeconds(2));
            await worker.StopAsync(stop.Token);
        }
    }

    private static ServiceProvider BuildProvider(IEmailNotificationService service)
    {
        var services = new ServiceCollection();
        services.AddSingleton<IEmailNotificationService>(service);
        return services.BuildServiceProvider();
    }

    private static IConfiguration BuildConfiguration(int recoveryIntervalSeconds)
        => new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["EmailNotificationWorker:BatchSize"] = "10",
                ["EmailNotificationWorker:RecoveryIntervalSeconds"] = recoveryIntervalSeconds.ToString()
            })
            .Build();

    private sealed class CountingNotificationService : IEmailNotificationService
    {
        private readonly SemaphoreSlim _calls = new(0);
        private int _callCount;

        public int CallCount => Volatile.Read(ref _callCount);

        public Task<int> ProcessPendingNotificationsAsync(int batchSize)
        {
            Interlocked.Increment(ref _callCount);
            _calls.Release();
            return Task.FromResult(0);
        }

        public async Task WaitForCallAsync(TimeSpan timeout)
        {
            using var cancellation = new CancellationTokenSource(timeout);
            await _calls.WaitAsync(cancellation.Token);
        }
    }
}
