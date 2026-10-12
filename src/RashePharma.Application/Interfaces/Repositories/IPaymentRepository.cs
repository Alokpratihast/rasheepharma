using RashePharma.Domain.Entities;

namespace RashePharma.Application.Interfaces.Repositories;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment);

    Task<Payment?> GetByIdAsync(int id);

    Task<Payment?> GetByStripeSessionIdAsync(string stripeSessionId);

    Task<Payment?> GetByStripePaymentIntentIdAsync(string stripePaymentIntentId);

    Task<Payment?> GetByCheckoutIdempotencyKeyAsync(
        string checkoutIdempotencyKey);

    Task<Payment?> GetActiveCheckoutByOrderIdAsync(int orderId);

    Task UpdateAsync(Payment payment);
}