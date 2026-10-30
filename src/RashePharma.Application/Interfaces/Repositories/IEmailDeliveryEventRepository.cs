
using RashePharma.Domain.Entities.Email;

namespace RashePharma.Application.Interfaces.Repositories;

public interface IEmailDeliveryEventRepository
{
    Task<EmailDeliveryEvent?> GetByIdAsync(Guid id);

    Task AddAsync(EmailDeliveryEvent deliveryEvent);
}
