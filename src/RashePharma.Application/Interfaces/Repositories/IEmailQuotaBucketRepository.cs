
using RashePharma.Domain.Entities.Email;

namespace RashePharma.Application.Interfaces.Repositories;

public interface IEmailQuotaBucketRepository
{
    Task<EmailQuotaBucket?> GetByIdAsync(Guid id);

    Task AddAsync(EmailQuotaBucket quotaBucket);

    Task UpdateAsync(EmailQuotaBucket quotaBucket);
}
