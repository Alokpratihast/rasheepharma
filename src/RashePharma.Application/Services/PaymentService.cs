using Microsoft.Extensions.Configuration;
using RashePharma.Application.DTOs.Payments;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Domain.Entities;
using Stripe;
using Stripe.Checkout;

namespace RashePharma.Application.Services;

public class PaymentService : IPaymentService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly ICartRepository _cartRepository;
    private readonly IStripeWebhookEventRepository _stripeWebhookEventRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;

    public PaymentService(
        IOrderRepository orderRepository,
        IPaymentRepository paymentRepository,
        ICartRepository cartRepository,
        IStripeWebhookEventRepository stripeWebhookEventRepository,
        IUnitOfWork unitOfWork,
        IConfiguration configuration)
    {
        _orderRepository = orderRepository;
        _paymentRepository = paymentRepository;
        _cartRepository = cartRepository;
        _stripeWebhookEventRepository = stripeWebhookEventRepository;
        _unitOfWork = unitOfWork;
        _configuration = configuration;
    }

    public async Task<string> CreateCheckoutSessionAsync(
        int userId,
        CreateCheckoutSessionRequest request)
    {
        var order = await _orderRepository
            .GetByIdAsync(request.OrderId);

        if (order == null)
        {
            throw new InvalidOperationException(
                "Order not found.");
        }

        if (order.UserId != userId)
        {
            throw new UnauthorizedAccessException(
                "You are not authorized to pay for this order.");
        }

        if (order.Status != "Pending")
        {
            throw new InvalidOperationException(
                "Only pending orders can be paid.");
        }

        if (!string.Equals(
                order.Currency,
                "USD",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Only USD payments are supported.");
        }

        if (order.TotalAmount < 200m)
        {
            throw new InvalidOperationException(
                "Minimum order value is $200 USD.");
        }

        if (order.Items == null || !order.Items.Any())
        {
            throw new InvalidOperationException(
                "Order has no items.");
        }

        var lineItems = order.Items.Select(item =>
            new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    Currency = "usd",

                    ProductData =
                        new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.ProductName,
                            Description =
                                $"{item.Strength} - {item.PackSize}"
                        },

                    UnitAmount =
                        checked((long)(item.UnitPrice * 100m))
                },

                Quantity = item.Quantity
            }).ToList();

        var calculatedStripeAmount =
            lineItems.Sum(item =>
                item.PriceData!.UnitAmount!.Value *
                item.Quantity!.Value);

        var expectedStripeAmount =
            checked((long)(order.TotalAmount * 100m));

        if (calculatedStripeAmount != expectedStripeAmount)
        {
            throw new InvalidOperationException(
                "Order total does not match the checkout amount.");
        }

        var frontendBaseUrl =
            _configuration["Frontend:BaseUrl"];

        if (string.IsNullOrWhiteSpace(frontendBaseUrl))
        {
            throw new InvalidOperationException(
                "Frontend base URL is not configured.");
        }

        frontendBaseUrl =
            frontendBaseUrl.TrimEnd('/');

        var payment = new Payment
        {
            OrderId = order.Id,
            Amount = order.TotalAmount,
            Currency = "USD",
            Status = "Pending",
            PaymentMethod = "Stripe",
            CreatedAt = DateTime.UtcNow
        };

        await _paymentRepository.AddAsync(payment);

        await _unitOfWork.SaveChangesAsync();

        try
        {
            var options = new SessionCreateOptions
            {
                Mode = "payment",

                LineItems = lineItems,

                SuccessUrl =
                    $"{frontendBaseUrl}/payment/success?session_id={{CHECKOUT_SESSION_ID}}",

                CancelUrl =
                    $"{frontendBaseUrl}/payment/cancel",

                Metadata = new Dictionary<string, string>
                {
                    ["OrderId"] = order.Id.ToString(),
                    ["OrderNumber"] = order.OrderNumber,
                    ["PaymentId"] = payment.Id.ToString()
                }
            };

            var service = new SessionService();

            var session =
                await service.CreateAsync(options);

            payment.StripeSessionId = session.Id;
            payment.UpdatedAt = DateTime.UtcNow;

            await _paymentRepository.UpdateAsync(payment);

            await _unitOfWork.SaveChangesAsync();

            return session.Url;
        }
        catch
        {
            payment.Status = "Failed";
            payment.FailureReason =
                "Unable to create Stripe Checkout Session.";
            payment.UpdatedAt = DateTime.UtcNow;

            await _paymentRepository.UpdateAsync(payment);

            await _unitOfWork.SaveChangesAsync();

            throw;
        }
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
            stripeEvent =
                Stripe.EventUtility.ConstructEvent(
                    json,
                    stripeSignature,
                    webhookSecret,
                    throwOnApiVersionMismatch: false);
        }
        catch (StripeException ex)
        {
            throw new InvalidOperationException(
                "Invalid Stripe webhook signature.",
                ex);
        }

        if (string.IsNullOrWhiteSpace(stripeEvent.Id))
        {
            throw new InvalidOperationException(
                "Stripe webhook event ID is missing.");
        }

        var existingEvent =
            await _stripeWebhookEventRepository
                .GetByStripeEventIdAsync(stripeEvent.Id);

        if (existingEvent != null)
        {
            return;
        }

        /*
         * ============================================================
         * CHECKOUT SESSION EXPIRED
         * ============================================================
         *
         * Customer opened Checkout but did not complete payment.
         *
         * Payment -> Cancelled
         * Order   -> remains Pending
         * Cart    -> remains untouched
         */
        if (stripeEvent.Type == "checkout.session.expired")
        {
            var session =
                stripeEvent.Data.Object as Stripe.Checkout.Session;

            if (session == null)
            {
                throw new InvalidOperationException(
                    "Stripe Checkout Session is missing.");
            }

            if (session.Metadata == null ||
                !session.Metadata.TryGetValue(
                    "PaymentId",
                    out var paymentIdString) ||
                !int.TryParse(
                    paymentIdString,
                    out var paymentId))
            {
                throw new InvalidOperationException(
                    "Stripe PaymentId metadata is invalid.");
            }

            var payment =
                await _paymentRepository
                    .GetByIdAsync(paymentId);

            if (payment == null)
            {
                throw new InvalidOperationException(
                    "Payment record not found.");
            }

            if (payment.Status == "Completed")
            {
                return;
            }

            await using var transaction =
                await _unitOfWork.BeginTransactionAsync();

            try
            {
                payment.Status = "Cancelled";
                payment.FailureReason =
                    "Stripe Checkout Session expired.";
                payment.UpdatedAt = DateTime.UtcNow;

                await _paymentRepository
                    .UpdateAsync(payment);

                var webhookEvent = new StripeWebhookEvent
                {
                    StripeEventId = stripeEvent.Id,
                    EventType = stripeEvent.Type,
                    CreatedAt = DateTime.UtcNow,
                    ProcessedAt = DateTime.UtcNow
                };

                await _stripeWebhookEventRepository
                    .AddAsync(webhookEvent);

                await _unitOfWork.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return;
        }

        /*
         * ============================================================
         * ASYNC PAYMENT FAILED
         * ============================================================
         *
         * Stripe reports that an asynchronous payment failed.
         *
         * Payment -> Failed
         * Order   -> remains Pending
         * Cart    -> remains untouched
         */
        if (stripeEvent.Type == "checkout.session.async_payment_failed")
        {
            var session =
                stripeEvent.Data.Object as Stripe.Checkout.Session;

            if (session == null)
            {
                throw new InvalidOperationException(
                    "Stripe Checkout Session is missing.");
            }

            if (session.Metadata == null ||
                !session.Metadata.TryGetValue(
                    "PaymentId",
                    out var paymentIdString) ||
                !int.TryParse(
                    paymentIdString,
                    out var paymentId))
            {
                throw new InvalidOperationException(
                    "Stripe PaymentId metadata is invalid.");
            }

            var payment =
                await _paymentRepository
                    .GetByIdAsync(paymentId);

            if (payment == null)
            {
                throw new InvalidOperationException(
                    "Payment record not found.");
            }

            if (payment.Status == "Completed")
            {
                return;
            }

            await using var transaction =
                await _unitOfWork.BeginTransactionAsync();

            try
            {
                payment.Status = "Failed";
                payment.FailureReason =
                    "Stripe payment failed.";
                payment.UpdatedAt = DateTime.UtcNow;

                await _paymentRepository
                    .UpdateAsync(payment);

                var webhookEvent = new StripeWebhookEvent
                {
                    StripeEventId = stripeEvent.Id,
                    EventType = stripeEvent.Type,
                    CreatedAt = DateTime.UtcNow,
                    ProcessedAt = DateTime.UtcNow
                };

                await _stripeWebhookEventRepository
                    .AddAsync(webhookEvent);

                await _unitOfWork.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

            return;
        }

        /*
         * ============================================================
         * UNKNOWN / CURRENTLY UNSUPPORTED EVENT
         * ============================================================
         *
         * We record the event for audit/idempotency purposes,
         * but do not modify payment or order state.
         */
        if (stripeEvent.Type != "checkout.session.completed")
        {
            var ignoredEvent = new StripeWebhookEvent
            {
                StripeEventId = stripeEvent.Id,
                EventType = stripeEvent.Type,
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = DateTime.UtcNow
            };

            await _stripeWebhookEventRepository
                .AddAsync(ignoredEvent);

            await _unitOfWork.SaveChangesAsync();

            return;
        }

        /*
         * ============================================================
         * CHECKOUT SESSION COMPLETED
         * ============================================================
         *
         * Successful Stripe payment.
         *
         * Payment -> Completed
         * Order   -> Paid
         * History -> Paid
         * Cart    -> Clear
         */
        var completedSession =
            stripeEvent.Data.Object as Stripe.Checkout.Session;

        if (completedSession == null)
        {
            throw new InvalidOperationException(
                "Stripe Checkout Session is missing.");
        }

        if (completedSession.Metadata == null)
        {
            throw new InvalidOperationException(
                "Stripe Checkout Session metadata is missing.");
        }

        if (!completedSession.Metadata.TryGetValue(
                "PaymentId",
                out var completedPaymentIdString) ||
            !int.TryParse(
                completedPaymentIdString,
                out var completedPaymentId))
        {
            throw new InvalidOperationException(
                "Stripe PaymentId metadata is invalid.");
        }

        if (!completedSession.Metadata.TryGetValue(
                "OrderId",
                out var completedOrderIdString) ||
            !int.TryParse(
                completedOrderIdString,
                out var completedOrderId))
        {
            throw new InvalidOperationException(
                "Stripe OrderId metadata is invalid.");
        }

        if (!completedSession.Metadata.TryGetValue(
                "OrderNumber",
                out var completedOrderNumber))
        {
            throw new InvalidOperationException(
                "Stripe OrderNumber metadata is missing.");
        }

        if (string.IsNullOrWhiteSpace(completedSession.Id))
        {
            throw new InvalidOperationException(
                "Stripe Session ID is missing.");
        }

        if (!string.Equals(
                completedSession.Currency,
                "usd",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Stripe payment currency must be USD.");
        }

        if (!string.Equals(
                completedSession.PaymentStatus,
                "paid",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Stripe payment has not been completed.");
        }

        if (string.IsNullOrWhiteSpace(
                completedSession.PaymentIntentId))
        {
            throw new InvalidOperationException(
                "Stripe PaymentIntent ID is missing.");
        }

        var completedPayment =
            await _paymentRepository
                .GetByIdAsync(completedPaymentId);

        if (completedPayment == null)
        {
            throw new InvalidOperationException(
                "Payment record not found.");
        }

        if (completedPayment.OrderId != completedOrderId)
        {
            throw new InvalidOperationException(
                "Payment does not belong to the specified order.");
        }

        if (!string.Equals(
                completedPayment.StripeSessionId,
                completedSession.Id,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Stripe Session does not match the payment record.");
        }

        if (!string.Equals(
                completedPayment.Currency,
                "USD",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Payment currency must be USD.");
        }

        if (completedPayment.Status == "Completed")
        {
            var alreadyCompletedEvent =
                new StripeWebhookEvent
                {
                    StripeEventId = stripeEvent.Id,
                    EventType = stripeEvent.Type,
                    CreatedAt = DateTime.UtcNow,
                    ProcessedAt = DateTime.UtcNow
                };

            await _stripeWebhookEventRepository
                .AddAsync(alreadyCompletedEvent);

            await _unitOfWork.SaveChangesAsync();

            return;
        }

        var order =
            await _orderRepository
                .GetByIdAsync(completedPayment.OrderId);

        if (order == null)
        {
            throw new InvalidOperationException(
                "Order associated with payment was not found.");
        }

        if (order.Id != completedOrderId)
        {
            throw new InvalidOperationException(
                "Stripe order does not match the payment order.");
        }

        if (!string.Equals(
                order.OrderNumber,
                completedOrderNumber,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Stripe order number does not match the order.");
        }

        if (!string.Equals(
                order.Currency,
                "USD",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "Order currency must be USD.");
        }

        if (order.TotalAmount < 200m)
        {
            throw new InvalidOperationException(
                "Order does not meet the minimum payment amount.");
        }

        if (!completedSession.AmountTotal.HasValue)
        {
            throw new InvalidOperationException(
                "Stripe payment amount is missing.");
        }

        var expectedAmount =
            checked((long)(order.TotalAmount * 100m));

        if (completedSession.AmountTotal.Value != expectedAmount)
        {
            throw new InvalidOperationException(
                "Stripe payment amount does not match the order total.");
        }

        if (completedPayment.StripePaymentIntentId != null &&
            !string.Equals(
                completedPayment.StripePaymentIntentId,
                completedSession.PaymentIntentId,
                StringComparison.Ordinal))
        {
            throw new InvalidOperationException(
                "Stripe PaymentIntent does not match the payment record.");
        }

        await using var completedTransaction =
            await _unitOfWork.BeginTransactionAsync();

        try
        {
            completedPayment.Status = "Completed";
            completedPayment.PaymentMethod = "Stripe";
            completedPayment.StripePaymentIntentId =
                completedSession.PaymentIntentId;
            completedPayment.StripeCustomerId =
                completedSession.CustomerId;
            completedPayment.UpdatedAt =
                DateTime.UtcNow;

            await _paymentRepository
                .UpdateAsync(completedPayment);

            order.Status = "Paid";
            order.UpdatedAt =
                DateTime.UtcNow;

            await _orderRepository
                .UpdateAsync(order);

            var statusHistory = new OrderStatusHistory
            {
                OrderId = order.Id,
                Status = "Paid",
                Comment =
                    "Payment completed successfully through Stripe.",
                CreatedAt = DateTime.UtcNow
            };

            await _orderRepository
                .AddStatusHistoryAsync(statusHistory);

            var cart =
                await _cartRepository
                    .GetByUserIdAsync(order.UserId);

            if (cart != null)
            {
                await _cartRepository
                    .ClearItemsAsync(cart.Id);
            }

            var webhookEvent = new StripeWebhookEvent
            {
                StripeEventId = stripeEvent.Id,
                EventType = stripeEvent.Type,
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = DateTime.UtcNow
            };

            await _stripeWebhookEventRepository
                .AddAsync(webhookEvent);

            await _unitOfWork.SaveChangesAsync();

            await completedTransaction.CommitAsync();
        }
        catch
        {
            await completedTransaction.RollbackAsync();
            throw;
        }
    }
}