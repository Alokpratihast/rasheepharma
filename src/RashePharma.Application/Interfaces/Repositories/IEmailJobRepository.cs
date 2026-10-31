
using RashePharma.Domain.Entities.Email;

namespace RashePharma.Application.Interfaces.Repositories;

public interface IEmailJobRepository
{
    Task<EmailJob?> GetByIdAsync(Guid id);

    Task<EmailJob?> GetByIdempotencyKeyAsync(
        string idempotencyKey);

    Task AddAsync(EmailJob emailJob);

    Task UpdateAsync(EmailJob emailJob);
}
