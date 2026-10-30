
using RashePharma.Domain.Entities.Email;

namespace RashePharma.Application.Interfaces.Repositories;

public interface IEmailQuotaReservationRepository
{
    Task<EmailQuotaReservation?> GetByIdAsync(Guid id);

    Task AddAsync(EmailQuotaReservation reservation);

    Task UpdateAsync(EmailQuotaReservation reservation);
}
