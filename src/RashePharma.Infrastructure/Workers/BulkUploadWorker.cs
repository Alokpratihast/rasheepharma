using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Domain.Enums;

namespace RashePharma.Infrastructure.Workers;

public class BulkUploadWorker : BackgroundService
{
    private static readonly TimeSpan PollingInterval =
        TimeSpan.FromSeconds(30);

    private const int MaxJobsPerCycle = 1;

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BulkUploadWorker> _logger;

    public BulkUploadWorker(
        IServiceScopeFactory scopeFactory,
        ILogger<BulkUploadWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Bulk upload worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingJobsAsync(stoppingToken);
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
                    "Unexpected error occurred in bulk upload worker.");
            }

            try
            {
                await Task.Delay(
                    PollingInterval,
                    stoppingToken);
            }
            catch (OperationCanceledException)
                when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }

        _logger.LogInformation(
            "Bulk upload worker stopped.");
    }

    private async Task ProcessPendingJobsAsync(
        CancellationToken cancellationToken)
    {
        for (var i = 0; i < MaxJobsPerCycle; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            using var scope = _scopeFactory.CreateScope();

            var repository =
                scope.ServiceProvider
                    .GetRequiredService<IBulkUploadRepository>();

            var service =
                scope.ServiceProvider
                    .GetRequiredService<IBulkUploadService>();

            var job =
                await repository.ClaimNextPendingJobAsync(
                    cancellationToken);

            if (job == null)
            {
                return;
            }

            _logger.LogInformation(
                "Bulk upload job {JobId} claimed for processing.",
                job.Id);

            try
            {
                await service.ProcessAsync(
                    job.Id,
                    cancellationToken);

                _logger.LogInformation(
                    "Bulk upload job {JobId} processing completed.",
                    job.Id);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Bulk upload job {JobId} failed during processing.",
                    job.Id);
            }
        }
    }
}