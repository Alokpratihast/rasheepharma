using RashePharma.Application.DTOs.Payments;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Domain.Entities;
using Stripe.Checkout;
using RashePharma.Application.Interfaces;
using Stripe;
namespace RashePharma.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PaymentService(
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<string> CreateCheckoutSessionAsync(
        int userId,
        CreateCheckoutSessionRequest request)
    {
        // 1. Get order
        var order = await _orderRepository
            .GetByIdAsync(request.OrderId);

        // 2. Validate order
        if (order == null)
        {
            throw new InvalidOperationException(
                "Order not found.");
        }

        // 3. Security check
        if (order.UserId != userId)
        {
            throw new UnauthorizedAccessException(
                "You are not authorized to pay for this order.");
        }

        // 4. Validate order status
        if (order.Status != "Pending")
        {
            throw new InvalidOperationException(
                "Only pending orders can be paid.");
        }

        // 5. Create Payment record
        var payment = new Payment
        {
            OrderId = order.Id,
            Amount = order.TotalAmount,
            Currency = order.Currency,
            Status = "Pending",
            PaymentMethod = "Stripe",
            CreatedAt = DateTime.UtcNow
        };

        await _paymentRepository.AddAsync(payment);

        // Save payment first so we have Payment.Id
        await _unitOfWork.SaveChangesAsync();

        // 6. Create Stripe line items
        var lineItems = order.Items.Select(item =>
            new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = order.Currency.ToLower(),

                    ProductData =
                        new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.ProductName,
                            Description =
                                $"{item.Strength} - {item.PackSize}"
                        },

                    UnitAmount =
                        (long)(item.UnitPrice * 100)
                },

                Quantity = item.Quantity
            }).ToList();

        // 7. Create Stripe Checkout Session
        var options = new SessionCreateOptions
        {
            Mode = "payment",

            LineItems = lineItems,

            SuccessUrl =
                "http://localhost:3000/payment/success?session_id={CHECKOUT_SESSION_ID}",

            CancelUrl =
                "http://localhost:3000/payment/cancel",

            Metadata = new Dictionary<string, string>
            {
                ["OrderId"] = order.Id.ToString(),
                ["OrderNumber"] = order.OrderNumber,
                ["PaymentId"] = payment.Id.ToString()
            }
        };

        var service = new SessionService();

        var session = await service.CreateAsync(options);

        // 8. Save Stripe Session ID
        payment.StripeSessionId = session.Id;
        payment.UpdatedAt = DateTime.UtcNow;

        await _paymentRepository.UpdateAsync(payment);

        await _unitOfWork.SaveChangesAsync();

        // 9. Return Stripe Checkout URL
        return session.Url;
    }


    public async Task HandleWebhookAsync(
        string json,
        string stripeSignature)
    {
        var webhookSecret =
            Environment.GetEnvironmentVariable(
                "Stripe__WebhookSecret");

        if (string.IsNullOrWhiteSpace(webhookSecret))
        {
            throw new InvalidOperationException(
                "Stripe webhook secret is not configured.");
        }

        Stripe.Event stripeEvent;

        try
        {
            stripeEvent = Stripe.EventUtility.ConstructEvent(
    json,
    stripeSignature,
    webhookSecret,
    throwOnApiVersionMismatch: false);
        }
        catch (StripeException ex)
        {
            throw new InvalidOperationException(
                "Invalid Stripe webhook signature.", ex);
        }

        if (stripeEvent.Type == "checkout.session.completed")
        {
            var session =
                stripeEvent.Data.Object as Stripe.Checkout.Session;

            if (session != null)
            {
                var paymentIdStr =
                    session.Metadata["PaymentId"];

                if (int.TryParse(paymentIdStr, out int paymentId))
                {
                    var payment =
                        await _paymentRepository
                            .GetByIdAsync(paymentId);

                    if (payment != null)
                    {
                        payment.Status = "Completed";
                        payment.UpdatedAt = DateTime.UtcNow;

                        await _paymentRepository
                            .UpdateAsync(payment);

                        await _unitOfWork.SaveChangesAsync();
                    }
                }
            }
        }
    }
}