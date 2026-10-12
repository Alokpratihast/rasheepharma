using RashePharma.Domain.Entities;

namespace RashePharma.Application.Interfaces.Repositories;

public interface IEmailNotificationRepository
{
    Task<EmailNotification?> GetByOrderAndTypeAsync(
        int orderId,
        string type);

    Task<EmailNotification?> GetByQuotationAndTypeAsync(
        int quotationId,
        string type);

    Task<List<EmailNotification>> GetPendingAsync(
        int batchSize);

    Task<List<EmailNotification>> ClaimPendingAsync(
        int batchSize);

    Task AddAsync(
        EmailNotification notification);

    Task UpdateAsync(
        EmailNotification notification);
}