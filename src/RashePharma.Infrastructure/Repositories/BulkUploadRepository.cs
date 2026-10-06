using Microsoft.EntityFrameworkCore;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Domain.Entities;
using RashePharma.Domain.Enums;
using RashePharma.Infrastructure.Data;

namespace RashePharma.Infrastructure.Repositories;

public class BulkUploadRepository : IBulkUploadRepository
{
    private readonly ApplicationDbContext _context;

    public BulkUploadRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BulkUploadJob> CreateJobAsync(
        BulkUploadJob job,
        CancellationToken cancellationToken = default)
    {
        await _context.BulkUploadJobs.AddAsync(
            job,
            cancellationToken);

        return job;
    }

    public async Task<BulkUploadJob?> GetByIdAsync(
        int jobId,
        CancellationToken cancellationToken = default)
    {
        return await _context.BulkUploadJobs
            .Include(j => j.Errors)
            .Include(j => j.Files)
            .FirstOrDefaultAsync(
                j => j.Id == jobId,
                cancellationToken);
    }

    public async Task<BulkUploadJob?> ClaimNextPendingJobAsync(
        CancellationToken cancellationToken = default)
    {
        var job = await _context.BulkUploadJobs
            .Where(j => j.Status == BulkUploadStatus.Pending)
            .OrderBy(j => j.CreatedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (job == null)
        {
            return null;
        }

        job.Status = BulkUploadStatus.Processing;
        job.StartedAt = DateTime.UtcNow;

        return job;
    }

    public async Task UpdateJobAsync(
        BulkUploadJob job,
        CancellationToken cancellationToken = default)
    {
        _context.BulkUploadJobs.Update(job);

        await Task.CompletedTask;
    }

    public async Task AddErrorAsync(
        BulkUploadError error,
        CancellationToken cancellationToken = default)
    {
        await _context.BulkUploadErrors.AddAsync(
            error,
            cancellationToken);
    }

    public async Task AddFileAsync(
        BulkUploadFile file,
        CancellationToken cancellationToken = default)
    {
        await _context.BulkUploadFiles.AddAsync(
            file,
            cancellationToken);
    }
}