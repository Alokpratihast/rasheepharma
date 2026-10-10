using Microsoft.EntityFrameworkCore;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;

namespace RashePharma.Infrastructure.Repositories;

public class EmailNotificationRepository : IEmailNotificationRepository
{
    private const int MaxAttempts = 5;

    private static readonly TimeSpan StaleProcessingTimeout =
        TimeSpan.FromMinutes(15);

    private readonly ApplicationDbContext _context;

    public EmailNotificationRepository(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmailNotification?> GetByOrderAndTypeAsync(
        int orderId,
        string type)
    {
        return await _context.EmailNotifications
            .FirstOrDefaultAsync(n =>
                n.OrderId == orderId &&
                n.Type == type);
    }

    public async Task<EmailNotification?> GetByQuotationAndTypeAsync(
        int quotationId,
        string type)
    {
        return await _context.EmailNotifications
            .FirstOrDefaultAsync(n =>
                n.QuotationId == quotationId &&
                n.Type == type);
    }

    public async Task<List<EmailNotification>> GetPendingAsync(
        int batchSize)
    {
        if (batchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(batchSize),
                "Batch size must be greater than zero.");
        }

        var now = DateTime.UtcNow;
        var staleProcessingTime =
            now - StaleProcessingTimeout;

        return await _context.EmailNotifications
            .Where(n =>
                n.Status == "Pending"
                ||
                (
                    n.Status == "Failed" &&
                    n.AttemptCount < MaxAttempts &&
                    n.LastAttemptAt != null
                )
                ||
                (
                    n.Status == "Processing" &&
                    n.AttemptCount < MaxAttempts &&
                    n.LastAttemptAt != null &&
                    n.LastAttemptAt <= staleProcessingTime
                ))
            .OrderBy(n => n.CreatedAt)
            .Take(batchSize)
            .ToListAsync();
    }

    public async Task<List<EmailNotification>> ClaimPendingAsync(
        int batchSize)
    {
        if (batchSize <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(batchSize),
                "Batch size must be greater than zero.");
        }

        var now = DateTime.UtcNow;

        var staleProcessingTime =
            now - StaleProcessingTimeout;

        await using var transaction =
            await _context.Database.BeginTransactionAsync();

        try
        {
            var notifications =
                await _context.EmailNotifications
                    .FromSqlInterpolated($"""
                        SELECT TOP ({batchSize}) *
                        FROM EmailNotifications
                            WITH (UPDLOCK, READPAST, ROWLOCK)
                        WHERE
                            Status = 'Pending'
                            OR
                            (
                                Status = 'Failed'
                                AND AttemptCount < {MaxAttempts}
                                AND LastAttemptAt IS NOT NULL
                                AND
                                (
                                    (
                                        AttemptCount = 1
                                        AND LastAttemptAt <=
                                            DATEADD(
                                                MINUTE,
                                                -1,
                                                {now}
                                            )
                                    )
                                    OR
                                    (
                                        AttemptCount = 2
                                        AND LastAttemptAt <=
                                            DATEADD(
                                                MINUTE,
                                                -5,
                                                {now}
                                            )
                                    )
                                    OR
                                    (
                                        AttemptCount = 3
                                        AND LastAttemptAt <=
                                            DATEADD(
                                                MINUTE,
                                                -15,
                                                {now}
                                            )
                                    )
                                    OR
                                    (
                                        AttemptCount >= 4
                                        AND LastAttemptAt <=
                                            DATEADD(
                                                MINUTE,
                                                -30,
                                                {now}
                                            )
                                    )
                                )
                            )
                            OR
                            (
                                Status = 'Processing'
                                AND AttemptCount < {MaxAttempts}
                                AND LastAttemptAt IS NOT NULL
                                AND LastAttemptAt <=
                                    {staleProcessingTime}
                            )
                        ORDER BY CreatedAt
                        """)
                    .ToListAsync();

            foreach (var notification in notifications)
            {
                notification.Status = "Processing";
                notification.AttemptCount++;
                notification.LastAttemptAt = now;
                notification.ErrorMessage = null;
            }

            await _context.SaveChangesAsync();

            await transaction.CommitAsync();

            return notifications;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task AddAsync(
        EmailNotification notification)
    {
        await _context.EmailNotifications
            .AddAsync(notification);
    }

    public async Task UpdateAsync(
        EmailNotification notification)
    {
        _context.EmailNotifications.Update(notification);

        await Task.CompletedTask;
    }
}