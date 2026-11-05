using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Domain.Entities.Email;
using RashePharma.Infrastructure.Configurations;
using RashePharma.Infrastructure.Data;

namespace RashePharma.Infrastructure.Services;

/// <summary>Atomically reserves the configured UTC-day Brevo allowance for outreach only.</summary>
public sealed class EmailOutreachQuotaService(
    ApplicationDbContext db,
    IOptions<EmailQuotaOptions> options) : IEmailOutreachQuotaService
{
    private const string Provider = "Brevo";

    public async Task<EmailQuotaReserveResult> ReserveAsync(
        Guid emailJobId,
        int attemptNumber,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        if (emailJobId == Guid.Empty || attemptNumber <= 0)
            throw new ArgumentOutOfRangeException(nameof(attemptNumber));

        var limit = options.Value.OutreachDailyLimit;
        if (limit <= 0)
            throw new InvalidOperationException("EmailQuota:OutreachDailyLimit must be greater than zero.");

        nowUtc = AsUtc(nowUtc);
        var start = nowUtc.Date;
        var end = start.AddDays(1);

        await using var transaction = await db.Database.BeginTransactionAsync(
            IsolationLevel.Serializable, cancellationToken);

        var existing = await db.EmailQuotaReservations
            .FirstOrDefaultAsync(row => row.EmailJobId == emailJobId && row.AttemptNumber == attemptNumber, cancellationToken);
        if (existing is not null)
        {
            await transaction.CommitAsync(cancellationToken);
            return existing.Status is "Reserved" or "Consumed"
                ? EmailQuotaReserveResult.AlreadyReserved
                : EmailQuotaReserveResult.LimitReached;
        }

        // Serializable isolation plus the unique provider/window index protects the absent-row case.
        var bucket = await db.EmailQuotaBuckets
            .FromSqlInterpolated($"SELECT * FROM [EmailQuotaBuckets] WITH (UPDLOCK, HOLDLOCK) WHERE [Provider] = {Provider} AND [WindowStartUtc] = {start} AND [WindowEndUtc] = {end}")
            .FirstOrDefaultAsync(cancellationToken);

        if (bucket is null)
        {
            bucket = new EmailQuotaBucket
            {
                Provider = Provider,
                WindowStartUtc = start,
                WindowEndUtc = end,
                DailyLimit = limit,
                ReservedCount = 0,
                CreatedAtUtc = nowUtc,
                UpdatedAtUtc = nowUtc
            };
            db.EmailQuotaBuckets.Add(bucket);
            await db.SaveChangesAsync(cancellationToken);
        }

        // Follow the stricter of an existing bucket's limit and the current setting;
        // a mid-day config increase never silently expands an already-created bucket.
        var effectiveLimit = Math.Min(bucket.DailyLimit, limit);
        if (bucket.ReservedCount >= effectiveLimit)
        {
            await transaction.CommitAsync(cancellationToken);
            return EmailQuotaReserveResult.LimitReached;
        }

        bucket.ReservedCount++;
        bucket.UpdatedAtUtc = nowUtc;
        db.EmailQuotaReservations.Add(new EmailQuotaReservation
        {
            EmailJobId = emailJobId,
            QuotaBucketId = bucket.Id,
            AttemptNumber = attemptNumber,
            Status = "Reserved",
            ReservedAtUtc = nowUtc
        });

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return EmailQuotaReserveResult.Reserved;
    }

    public async Task ConsumeAsync(Guid emailJobId, int attemptNumber, DateTime nowUtc, CancellationToken cancellationToken = default)
    {
        var reservation = await db.EmailQuotaReservations
            .FirstOrDefaultAsync(row => row.EmailJobId == emailJobId && row.AttemptNumber == attemptNumber, cancellationToken);
        if (reservation is null || reservation.Status != "Reserved") return;
        reservation.Status = "Consumed";
        reservation.ConsumedAtUtc = AsUtc(nowUtc);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task ReleaseAsync(Guid emailJobId, int attemptNumber, DateTime nowUtc, CancellationToken cancellationToken = default)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        var reservation = await db.EmailQuotaReservations
            .FromSqlInterpolated($"SELECT * FROM [EmailQuotaReservations] WITH (UPDLOCK, ROWLOCK) WHERE [EmailJobId] = {emailJobId} AND [AttemptNumber] = {attemptNumber}")
            .FirstOrDefaultAsync(cancellationToken);
        if (reservation is null || reservation.Status != "Reserved")
        {
            await transaction.CommitAsync(cancellationToken);
            return;
        }

        var bucket = await db.EmailQuotaBuckets
            .FromSqlInterpolated($"SELECT * FROM [EmailQuotaBuckets] WITH (UPDLOCK, ROWLOCK) WHERE [Id] = {reservation.QuotaBucketId}")
            .FirstAsync(cancellationToken);
        reservation.Status = "Released";
        reservation.ReleasedAtUtc = AsUtc(nowUtc);
        bucket.ReservedCount = Math.Max(0, bucket.ReservedCount - 1);
        bucket.UpdatedAtUtc = AsUtc(nowUtc);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private static DateTime AsUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
