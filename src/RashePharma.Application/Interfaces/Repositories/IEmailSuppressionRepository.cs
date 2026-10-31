
using RashePharma.Domain.Entities.Email;

namespace RashePharma.Application.Interfaces.Repositories;

public interface IEmailSuppressionRepository
{
    Task<EmailSuppression?> GetByIdAsync(Guid id);

    Task<EmailSuppression?> GetByEmailAsync(string email);

    Task AddAsync(EmailSuppression suppression);

    Task UpdateAsync(EmailSuppression suppression);
}
