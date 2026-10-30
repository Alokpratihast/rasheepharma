
using RashePharma.Domain.Entities.Email;

namespace RashePharma.Application.Interfaces.Repositories;

public interface IEmailOutboxRepository
{
    Task<EmailOutboxMessage?> GetByIdAsync(Guid id);

    Task AddAsync(EmailOutboxMessage outboxMessage);

    Task UpdateAsync(EmailOutboxMessage outboxMessage);

    Task<IReadOnlyList<EmailOutboxMessage>> ClaimPendingAsync(
    int batchSize,
    DateTime nowUtc,
    TimeSpan leaseDuration,
    CancellationToken cancellationToken = default);
}
