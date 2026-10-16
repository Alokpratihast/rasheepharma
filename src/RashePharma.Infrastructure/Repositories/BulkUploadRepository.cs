
using Microsoft.EntityFrameworkCore;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Domain.Entities;
using RashePharma.Domain.Enums;
using RashePharma.Infrastructure.Data;

namespace RashePharma.Infrastructure.Repositories;

/// <summary>
/// Provides database operations for bulk-upload jobs, including job claiming,
/// lease renewal, ownership validation, and persistence of job-related records.
/// </summary>
public class BulkUploadRepository : IBulkUploadRepository
{
    private readonly ApplicationDbContext _context;

    /// <summary>
    /// Initializes the repository with the application's database context.
    /// </summary>
    /// <param name="context">
    /// Entity Framework Core context used to access bulk-upload data.
    /// </param>
    public BulkUploadRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Adds a new bulk-upload job to the current database context.
    /// </summary>
    /// <param name="job">The job entity to add.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>The job entity being tracked by Entity Framework.</returns>
    /// <remarks>
    /// This method does not commit the transaction or call SaveChangesAsync.
    /// The caller controls when the changes are persisted.
    /// </remarks>
    public async Task<BulkUploadJob> CreateJobAsync(
        BulkUploadJob job,
        CancellationToken cancellationToken = default)
    {
        await _context.BulkUploadJobs.AddAsync(job, cancellationToken);
        return job;
    }

    /// <summary>
    /// Retrieves a job together with its associated errors and uploaded files.
    /// </summary>
    /// <param name="jobId">The database identifier of the job.</param>
    /// <param name="cancellationToken">Token used to cancel the query.</param>
    /// <returns>
    /// The job with its errors and files, or null if the job does not exist.
    /// </returns>
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

    /// <summary>
    /// Atomically claims the oldest eligible bulk-upload job.
    /// </summary>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>
    /// The claimed job, or null when no eligible job is available.
    /// </returns>
    /// <remarks>
    /// A job is eligible when it is Pending, or when it is Processing
    /// and its lease has expired or is missing.
    ///
    /// UPDLOCK requests update locks while selecting the candidate.
    /// READPAST allows a competing worker to skip locked rows.
    /// ROWLOCK requests row-level locking but does not guarantee it.
    ///
    /// A fresh claim token identifies the new owner. The lease and ownership
    /// metadata are saved before the transaction commits.
    /// </remarks>
    public async Task<BulkUploadJob?> ClaimNextPendingJobAsync(
        CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                cancellationToken);

        var job = await _context.BulkUploadJobs
            .FromSqlInterpolated($"""
                SELECT TOP (1) *
                FROM [BulkUploadJobs] WITH (UPDLOCK, READPAST, ROWLOCK)
                WHERE
                    [Status] = {(int)BulkUploadStatus.Pending}
                    OR
                    (
                        [Status] = {(int)BulkUploadStatus.Processing}
                        AND
                        (
                            [LeaseExpiresAtUtc] IS NULL
                            OR [LeaseExpiresAtUtc] <= SYSUTCDATETIME()
                        )
                    )
                ORDER BY [CreatedAt], [Id]
                """)
            .FirstOrDefaultAsync(cancellationToken);

        if (job is null)
        {
            // No eligible job was found; close the transaction cleanly.
            await transaction.CommitAsync(cancellationToken);
            return null;
        }

        var now = DateTime.UtcNow;

        job.Status = BulkUploadStatus.Processing;
        job.ClaimToken = Guid.NewGuid().ToString("N");
        job.AttemptCount++;
        job.StartedAt ??= now;
        job.HeartbeatAtUtc = now;
        job.LeaseExpiresAtUtc = now.AddMinutes(2);
        job.ErrorMessage = null;

        // Persist the claim before releasing the row lock.
        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return job;
    }

    /// <summary>
    /// Extends the lease of a currently owned Processing job.
    /// </summary>
    /// <param name="jobId">The identifier of the job.</param>
    /// <param name="claimToken">The current owner's claim token.</param>
    /// <param name="leaseDuration">The duration to add to the current time.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>
    /// True if exactly one job was renewed; otherwise false.
    /// </returns>
    /// <remarks>
    /// The update succeeds only when the token matches and the existing
    /// lease has not expired. ExecuteUpdateAsync performs a direct database
    /// update without loading the entity into the change tracker.
    /// </remarks>
    public async Task<bool> TryRenewLeaseAsync(
        int jobId,
        string claimToken,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default)
    {
        if (leaseDuration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(leaseDuration),
                "Lease duration must be greater than zero.");
        }

        var now = DateTime.UtcNow;

        var affectedRows = await _context.BulkUploadJobs
            .Where(j =>
                j.Id == jobId &&
                j.Status == BulkUploadStatus.Processing &&
                j.ClaimToken == claimToken &&
                j.LeaseExpiresAtUtc > now)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(
                        j => j.HeartbeatAtUtc,
                        now)
                    .SetProperty(
                        j => j.LeaseExpiresAtUtc,
                        now.Add(leaseDuration)),
                cancellationToken);

        return affectedRows == 1;
    }

    /// <summary>
    /// Checks whether the supplied token still owns an unexpired job lease.
    /// </summary>
    /// <param name="jobId">The identifier of the job.</param>
    /// <param name="claimToken">The token to validate.</param>
    /// <param name="cancellationToken">Token used to cancel the query.</param>
    /// <returns>
    /// True if the job is Processing, the token matches, and the lease
    /// is still valid; otherwise false.
    /// </returns>
    public async Task<bool> IsClaimOwnerAsync(
        int jobId,
        string claimToken,
        CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        return await _context.BulkUploadJobs.AnyAsync(
            j =>
                j.Id == jobId &&
                j.Status == BulkUploadStatus.Processing &&
                j.ClaimToken == claimToken &&
                j.LeaseExpiresAtUtc > now,
            cancellationToken);
    }

    /// <summary>
    /// Saves tracked changes only when the supplied token still owns the job.
    /// </summary>
    /// <param name="job">The job entity to update.</param>
    /// <param name="claimToken">The current owner's claim token.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <returns>
    /// True if ownership was validated and changes were committed;
    /// false if the job is no longer owned by the supplied token.
    /// </returns>
    /// <remarks>
    /// The job row is locked while ownership is checked and changes are saved.
    /// SaveChangesAsync persists all pending changes tracked by this DbContext,
    /// not just changes to the supplied job.
    /// </remarks>
    public async Task<bool> UpdateJobIfOwnedAsync(
        BulkUploadJob job,
        string claimToken,
        CancellationToken cancellationToken = default)
    {
        await using var transaction =
            await _context.Database.BeginTransactionAsync(
                cancellationToken);

        var isOwner = await _context.BulkUploadJobs
            .FromSqlInterpolated($"""
                SELECT *
                FROM [BulkUploadJobs] WITH (UPDLOCK, HOLDLOCK)
                WHERE
                    [Id] = {job.Id}
                    AND [Status] = {(int)BulkUploadStatus.Processing}
                    AND [ClaimToken] = {claimToken}
                    AND [LeaseExpiresAtUtc] > SYSUTCDATETIME()
                """)
            .AsNoTracking()
            .AnyAsync(cancellationToken);

        if (!isOwner)
        {
            await transaction.RollbackAsync(cancellationToken);
            return false;
        }

        _context.BulkUploadJobs.Update(job);

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return true;
    }

    /// <summary>
    /// Marks a job entity as modified in the current database context.
    /// </summary>
    /// <param name="job">The job entity to update.</param>
    /// <param name="cancellationToken">Token retained for interface consistency.</param>
    /// <remarks>
    /// This method does not save changes itself. It is not ownership-aware
    /// and should not be used by the worker for lease-protected updates.
    /// </remarks>
    public async Task UpdateJobAsync(
        BulkUploadJob job,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _context.BulkUploadJobs.Update(job);
        await Task.CompletedTask;
    }

    /// <summary>
    /// Adds a row-level validation or processing error to the current context.
    /// </summary>
    /// <param name="error">The error record to add.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <remarks>
    /// The caller must save changes to persist the error record.
    /// </remarks>
    public async Task AddErrorAsync(
        BulkUploadError error,
        CancellationToken cancellationToken = default)
    {
        await _context.BulkUploadErrors.AddAsync(
            error,
            cancellationToken);
    }

    /// <summary>
    /// Adds an uploaded-file metadata record to the current context.
    /// </summary>
    /// <param name="file">The file metadata to add.</param>
    /// <param name="cancellationToken">Token used to cancel the operation.</param>
    /// <remarks>
    /// This adds database metadata only; it does not upload file bytes to storage.
    /// The caller must save changes to persist the record.
    /// </remarks>
    public async Task AddFileAsync(
        BulkUploadFile file,
        CancellationToken cancellationToken = default)
    {
        await _context.BulkUploadFiles.AddAsync(
            file,
            cancellationToken);
    }
}
