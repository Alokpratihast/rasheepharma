using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Moq;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Services;
using RashePharma.Domain.Entities;

namespace RashePharma.Tests.Unit;

public class PaymentWebhookTests
{
    private const string TestWebhookSecret =
        "whsec_test_payment_webhook_secret";

    private readonly Mock<IOrderRepository> _orderRepository = new();
    private readonly Mock<IPaymentRepository> _paymentRepository = new();
    private readonly Mock<ICartRepository> _cartRepository = new();
    private readonly Mock<IStripeWebhookEventRepository> _webhookRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<ITransaction> _transaction = new();
    private readonly Mock<IConfiguration> _configuration = new();

    private PaymentService CreateService()
    {
        return new PaymentService(
            _orderRepository.Object,
            _paymentRepository.Object,
            _cartRepository.Object,
            _webhookRepository.Object,
            _unitOfWork.Object,
            _configuration.Object);
    }

    [Fact]
    public async Task HandleWebhookAsync_ShouldCompletePaymentAndOrder()
    {
        // Arrange
        SetWebhookSecret();

        var order = CreateOrder();
        var payment = CreatePayment(order);

        var cart = new Cart
        {
            Id = 50,
            UserId = order.UserId
        };

        _paymentRepository
            .Setup(x => x.GetByIdAsync(payment.Id))
            .ReturnsAsync(payment);

        _orderRepository
            .Setup(x => x.GetByIdAsync(order.Id))
            .ReturnsAsync(order);

        _cartRepository
            .Setup(x => x.GetByUserIdAsync(order.UserId))
            .ReturnsAsync(cart);

        _webhookRepository
            .Setup(x => x.GetByStripeEventIdAsync(It.IsAny<string>()))
            .ReturnsAsync((StripeWebhookEvent?)null);

        _unitOfWork
            .Setup(x => x.BeginTransactionAsync())
            .ReturnsAsync(_transaction.Object);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var sessionId = payment.StripeSessionId!;
        var paymentIntentId = "pi_test_payment_001";

        var json = CreateCheckoutCompletedPayload(
            "evt_test_completed_001",
            sessionId,
            payment.Id,
            order.Id,
            order.OrderNumber,
            order.TotalAmount,
            paymentIntentId);

        var signature = CreateStripeSignature(
            json,
            TestWebhookSecret);

        var service = CreateService();

        // Act
        await service.HandleWebhookAsync(
            json,
            signature);

        // Assert
        Assert.Equal("Completed", payment.Status);
        Assert.Equal("Stripe", payment.PaymentMethod);
        Assert.Equal(
            paymentIntentId,
            payment.StripePaymentIntentId);

        Assert.Equal("Paid", order.Status);

        _paymentRepository.Verify(
            x => x.UpdateAsync(payment),
            Times.Once);

        _orderRepository.Verify(
            x => x.UpdateAsync(order),
            Times.Once);

        _orderRepository.Verify(
            x => x.AddStatusHistoryAsync(
                It.Is<OrderStatusHistory>(h =>
                    h.OrderId == order.Id &&
                    h.Status == "Paid")),
            Times.Once);

        _cartRepository.Verify(
            x => x.ClearItemsAsync(cart.Id),
            Times.Once);

        _webhookRepository.Verify(
            x => x.AddAsync(
                It.Is<StripeWebhookEvent>(e =>
                    e.EventType ==
                    "checkout.session.completed")),
            Times.Once);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(),
            Times.Once);

        _transaction.Verify(
            x => x.CommitAsync(),
            Times.Once);

        _transaction.Verify(
            x => x.RollbackAsync(),
            Times.Never);
    }

    [Fact]
    public async Task HandleWebhookAsync_ShouldCancelPayment_WhenSessionExpires()
    {
        // Arrange
        SetWebhookSecret();

        var order = CreateOrder();
        var payment = CreatePayment(order);

        _paymentRepository
            .Setup(x => x.GetByIdAsync(payment.Id))
            .ReturnsAsync(payment);

        _webhookRepository
            .Setup(x => x.GetByStripeEventIdAsync(It.IsAny<string>()))
            .ReturnsAsync((StripeWebhookEvent?)null);

        _unitOfWork
            .Setup(x => x.BeginTransactionAsync())
            .ReturnsAsync(_transaction.Object);

        _unitOfWork
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);

        var json = CreateCheckoutExpiredPayload(
            "evt_test_expired_001",
            payment.Id);

        var signature = CreateStripeSignature(
            json,
            TestWebhookSecret);

        var service = CreateService();

        // Act
        await service.HandleWebhookAsync(
            json,
            signature);

        // Assert
        Assert.Equal("Cancelled", payment.Status);

        Assert.Equal(
            "Stripe Checkout Session expired.",
            payment.FailureReason);

        Assert.Equal(
            "Pending",
            order.Status);

        _paymentRepository.Verify(
            x => x.UpdateAsync(payment),
            Times.Once);

        _webhookRepository.Verify(
            x => x.AddAsync(
                It.Is<StripeWebhookEvent>(e =>
                    e.EventType ==
                    "checkout.session.expired")),
            Times.Once);

        _transaction.Verify(
            x => x.CommitAsync(),
            Times.Once);

        _transaction.Verify(
            x => x.RollbackAsync(),
            Times.Never);
    }

    [Fact]
    public async Task HandleWebhookAsync_ShouldIgnoreDuplicateWebhook()
    {
        // Arrange
        SetWebhookSecret();

        var existingEvent = new StripeWebhookEvent
        {
            Id = 1,
            StripeEventId = "evt_duplicate_001",
            EventType = "checkout.session.completed",
            CreatedAt = DateTime.UtcNow,
            ProcessedAt = DateTime.UtcNow
        };

        _webhookRepository
            .Setup(x => x.GetByStripeEventIdAsync("evt_duplicate_001"))
            .ReturnsAsync(existingEvent);

        var json = CreateCheckoutCompletedPayload(
            "evt_duplicate_001",
            "cs_test_duplicate",
            1,
            1,
            "ORD-DUPLICATE",
            300m,
            "pi_duplicate");

        var signature = CreateStripeSignature(
            json,
            TestWebhookSecret);

        var service = CreateService();

        // Act
        await service.HandleWebhookAsync(
            json,
            signature);

        // Assert
        _paymentRepository.Verify(
            x => x.GetByIdAsync(It.IsAny<int>()),
            Times.Never);

        _orderRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Order>()),
            Times.Never);

        _paymentRepository.Verify(
            x => x.UpdateAsync(It.IsAny<Payment>()),
            Times.Never);

        _unitOfWork.Verify(
            x => x.SaveChangesAsync(),
            Times.Never);

        _unitOfWork.Verify(
            x => x.BeginTransactionAsync(),
            Times.Never);
    }

    [Fact]
    public async Task HandleWebhookAsync_ShouldRejectInvalidSignature()
    {
        // Arrange
        SetWebhookSecret();

        var json = CreateCheckoutExpiredPayload(
            "evt_invalid_signature",
            1);

        var service = CreateService();

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.HandleWebhookAsync(
                json,
                "t=1234567890,v1=invalid_signature"));

        // Assert
        Assert.Equal(
            "Invalid Stripe webhook signature.",
            exception.Message);
    }

    [Fact]
    public async Task HandleWebhookAsync_ShouldThrow_WhenWebhookSecretIsMissing()
    {
        // Arrange
        var previousSecret =
            Environment.GetEnvironmentVariable(
                "Stripe__WebhookSecret");

        try
        {
            Environment.SetEnvironmentVariable(
                "Stripe__WebhookSecret",
                null);

            var service = CreateService();

            // Act
            var exception =
                await Assert.ThrowsAsync<InvalidOperationException>(
                    () => service.HandleWebhookAsync(
                        "{}",
                        "invalid"));

            // Assert
            Assert.Equal(
                "Stripe webhook secret is not configured.",
                exception.Message);
        }
        finally
        {
            Environment.SetEnvironmentVariable(
                "Stripe__WebhookSecret",
                previousSecret);
        }
    }

    [Fact]
    public async Task HandleWebhookAsync_ShouldThrow_WhenPaymentDoesNotExist()
    {
        // Arrange
        SetWebhookSecret();

        _webhookRepository
            .Setup(x => x.GetByStripeEventIdAsync(It.IsAny<string>()))
            .ReturnsAsync((StripeWebhookEvent?)null);

        _paymentRepository
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((Payment?)null);

        var json = CreateCheckoutExpiredPayload(
            "evt_missing_payment",
            999);

        var signature = CreateStripeSignature(
            json,
            TestWebhookSecret);

        var service = CreateService();

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.HandleWebhookAsync(
                json,
                signature));

        // Assert
        Assert.Equal(
            "Payment record not found.",
            exception.Message);
    }

    private void SetWebhookSecret()
    {
        Environment.SetEnvironmentVariable(
            "Stripe__WebhookSecret",
            TestWebhookSecret);
    }

    private static Order CreateOrder()
    {
        var order = new Order
        {
            Id = 22,
            UserId = 5,
            OrderNumber = "ORD-TEST-022",
            TotalAmount = 300m,
            Currency = "USD",
            Status = "Pending"
        };

        order.Items.Add(
            new OrderItem
            {
                Id = 1,
                OrderId = order.Id,
                ProductVariantId = 10,
                ProductName = "Test Medicine",
                Strength = "500mg",
                PackSize = "10 Tablets",
                Quantity = 2,
                UnitPrice = 150m
            });

        return order;
    }

    private static Payment CreatePayment(Order order)
    {
        return new Payment
        {
            Id = 5,
            OrderId = order.Id,
            Amount = order.TotalAmount,
            Currency = "USD",
            Status = "Pending",
            PaymentMethod = "Stripe",
            StripeSessionId = "cs_test_payment_001",
            CreatedAt = DateTime.UtcNow,
            Order = order
        };
    }

    private static string CreateCheckoutCompletedPayload(
        string eventId,
        string sessionId,
        int paymentId,
        int orderId,
        string orderNumber,
        decimal amount,
        string paymentIntentId)
    {
        var amountInCents =
            checked((long)(amount * 100m));

        var payload = new
        {
            id = eventId,
            @object = "event",
            type = "checkout.session.completed",
            data = new
            {
                @object = new
                {
                    id = sessionId,
                    @object = "checkout.session",
                    currency = "usd",
                    payment_status = "paid",
                    payment_intent = paymentIntentId,
                    customer = "cus_test_customer",
                    amount_total = amountInCents,
                    metadata = new Dictionary<string, string>
                    {
                        ["PaymentId"] = paymentId.ToString(),
                        ["OrderId"] = orderId.ToString(),
                        ["OrderNumber"] = orderNumber
                    }
                }
            }
        };

        return JsonSerializer.Serialize(payload);
    }

    private static string CreateCheckoutExpiredPayload(
        string eventId,
        int paymentId)
    {
        var payload = new
        {
            id = eventId,
            @object = "event",
            type = "checkout.session.expired",
            data = new
            {
                @object = new
                {
                    id = "cs_test_expired_001",
                    @object = "checkout.session",
                    metadata = new Dictionary<string, string>
                    {
                        ["PaymentId"] = paymentId.ToString()
                    }
                }
            }
        };

        return JsonSerializer.Serialize(payload);
    }

    private static string CreateStripeSignature(
        string payload,
        string secret)
    {
        var timestamp =
            DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        var signedPayload =
            $"{timestamp}.{payload}";

        using var hmac =
            new HMACSHA256(
                Encoding.UTF8.GetBytes(secret));

        var hash =
            hmac.ComputeHash(
                Encoding.UTF8.GetBytes(signedPayload));

        var signature =
            Convert.ToHexString(hash)
                .ToLowerInvariant();

        return $"t={timestamp},v1={signature}";
    }
}