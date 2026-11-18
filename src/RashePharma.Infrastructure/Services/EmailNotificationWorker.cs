using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Infrastructure.Services;

public class EmailNotificationWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<EmailNotificationWorker> _logger;
    private readonly IEmailNotificationSignal _signal;

    private readonly int _batchSize;
    private readonly TimeSpan _recoveryInterval;

    public EmailNotificationWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<EmailNotificationWorker> logger,
        IConfiguration configuration,
        IEmailNotificationSignal signal)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _signal = signal;
        _batchSize = Math.Clamp(configuration.GetValue("EmailNotificationWorker:BatchSize", 10), 1, 100);
        _recoveryInterval = TimeSpan.FromSeconds(
            Math.Clamp(configuration.GetValue("EmailNotificationWorker:RecoveryIntervalSeconds", 300), 1, 3600));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Email notification worker started in signal-driven mode. Recovery interval: {RecoveryIntervalSeconds}s.",
            _recoveryInterval.TotalSeconds);

        // Run once at startup so notifications committed while the process
        // was offline are not dependent on receiving an in-memory signal.
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DrainPendingNotificationsAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "Email notification worker failed while draining pending notifications.");
            }

            try
            {
                // Signals provide low-latency processing. The timeout is the
                // durable recovery sweep for missed signals and delayed retries.
                await _signal.WaitForWorkAsync(_recoveryInterval, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation("Email notification worker stopped.");
    }

    private async Task DrainPendingNotificationsAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            int claimedCount;

            await using (var scope = _scopeFactory.CreateAsyncScope())
            {
                var notificationService =
                    scope.ServiceProvider.GetRequiredService<IEmailNotificationService>();

                claimedCount = await notificationService
                    .ProcessPendingNotificationsAsync(_batchSize);
            }

            // A short batch means the durable queue is currently drained.
            // A full batch may have more work, so immediately claim the next one.
            if (claimedCount < _batchSize)
                return;
        }
    }
}
