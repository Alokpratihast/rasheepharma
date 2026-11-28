using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Infrastructure.Workers;

public sealed class LocalBulkUploadStorageCleanupService : BackgroundService
{
    private static readonly TimeSpan Expiry = TimeSpan.FromHours(24);
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<LocalBulkUploadStorageCleanupService> _logger;

    public LocalBulkUploadStorageCleanupService(
        IServiceScopeFactory scopeFactory,
        ILogger<LocalBulkUploadStorageCleanupService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var scope = _scopeFactory.CreateAsyncScope();
                var storage = scope.ServiceProvider.GetRequiredService<IBulkUploadStorageService>();
                await storage.CleanupExpiredUploadsAsync(Expiry, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Local bulk-upload temporary-file cleanup failed.");
            }

            try
            {
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
