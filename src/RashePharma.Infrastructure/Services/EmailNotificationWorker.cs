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

    private readonly int _batchSize;
    private readonly TimeSpan _pollingInterval;

    public EmailNotificationWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<EmailNotificationWorker> logger,
        IConfiguration configuration)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _batchSize = Math.Clamp(configuration.GetValue("EmailNotificationWorker:BatchSize", 10), 1, 100);
        _pollingInterval = TimeSpan.FromSeconds(
            Math.Clamp(configuration.GetValue("EmailNotificationWorker:PollingIntervalSeconds", 120), 10, 3600));
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Email notification worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var scope =
                    _scopeFactory.CreateScope();

                var notificationService =
                    scope.ServiceProvider
                        .GetRequiredService<IEmailNotificationService>();

                await notificationService
                    .ProcessPendingNotificationsAsync(_batchSize);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error occurred while processing email notifications.");
            }

            try
            {
                await Task.Delay(
                    _pollingInterval,
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation(
            "Email notification worker stopped.");
    }
}