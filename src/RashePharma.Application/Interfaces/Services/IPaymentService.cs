using RashePharma.Application.DTOs.Payments;

namespace RashePharma.Application.Interfaces.Services;

public interface IPaymentService
{
    Task<string> CreateCheckoutSessionAsync(
        int userId,
        CreateCheckoutSessionRequest request);

    Task HandleWebhookAsync(
        string json,
        string stripeSignature);
}