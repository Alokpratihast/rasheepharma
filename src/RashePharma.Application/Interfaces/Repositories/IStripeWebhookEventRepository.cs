using RashePharma.Domain.Entities;

namespace RashePharma.Application.Interfaces.Repositories;

public interface IStripeWebhookEventRepository
{
    Task<StripeWebhookEvent?> GetByStripeEventIdAsync(
        string stripeEventId);

    Task AddAsync(StripeWebhookEvent webhookEvent);

    Task UpdateAsync(StripeWebhookEvent webhookEvent);
}