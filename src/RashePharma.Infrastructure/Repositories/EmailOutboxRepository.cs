
using Microsoft.EntityFrameworkCore;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Domain.Entities.Email;
using RashePharma.Infrastructure.Data;

namespace RashePharma.Infrastructure.Repositories;

public sealed class EmailOutboxRepository : IEmailOutboxRepository
{
    private readonly ApplicationDbContext _context;

    public EmailOutboxRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmailOutboxMessage?> GetByIdAsync(Guid id)
    {
        return await _context.EmailOutboxMessages
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(EmailOutboxMessage outboxMessage)
    {
        await _context.EmailOutboxMessages.AddAsync(outboxMessage);
    }

    public Task UpdateAsync(EmailOutboxMessage outboxMessage)
    {
        _context.EmailOutboxMessages.Update(outboxMessage);
        return Task.CompletedTask;
    }

    public async Task<IReadOnlyList<EmailOutboxMessage>> ClaimPendingAsync(
        int batchSize,
        DateTime nowUtc,
        TimeSpan leaseDuration,
        CancellationToken cancellationToken = default)
    {
        if (batchSize <= 0)
            throw new ArgumentOutOfRangeException(nameof(batchSize));

        if (leaseDuration <= TimeSpan.Zero)
            throw new ArgumentOutOfRangeException(nameof(leaseDuration));

        var leaseUntilUtc = nowUtc.Add(leaseDuration);

        await using var transaction =
            await _context.Database.BeginTransactionAsync(cancellationToken);

        var messages = await _context.EmailOutboxMessages
            .FromSqlInterpolated($@"
                SELECT TOP ({batchSize}) *
                FROM EmailOutboxMessages WITH (UPDLOCK, READPAST, ROWLOCK)
                WHERE
                (
                    (
                        Status = {EmailOutboxStatus.Pending}
                    )
                    OR
                    (
                        Status = {EmailOutboxStatus.RetryScheduled}
                        AND NextAttemptAtUtc IS NOT NULL
                        AND NextAttemptAtUtc <= {nowUtc}
                    )
                    OR
                    (
                        Status = {EmailOutboxStatus.Processing}
                        AND
                        (
                            LeaseUntilUtc IS NULL
                            OR LeaseUntilUtc <= {nowUtc}
                        )
                    )
                )
                AND
                (
                    LeaseUntilUtc IS NULL
                    OR LeaseUntilUtc <= {nowUtc}
                )
                ORDER BY CreatedAtUtc, Id")
            .ToListAsync(cancellationToken);

        foreach (var message in messages)
        {
            message.Status = EmailOutboxStatus.Processing;
            message.LeaseUntilUtc = leaseUntilUtc;
            message.AttemptCount++;
        }

        await _context.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return messages;
    }
}
