using Microsoft.Extensions.Configuration;
using Moq;
using RashePharma.Application.DTOs.Payments;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Services;
using RashePharma.Domain.Entities;

namespace RashePharma.Tests.Unit;

public class PaymentCheckoutTests
{
    private readonly Mock<IOrderRepository> _orderRepository = new();
    private readonly Mock<IPaymentRepository> _paymentRepository = new();
    private readonly Mock<ICartRepository> _cartRepository = new();
    private readonly Mock<IStripeWebhookEventRepository> _webhookRepository = new();
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
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
    public async Task CreateCheckoutSessionAsync_ShouldRejectOrderBelowMinimum()
    {
        // Arrange
        var order = CreateOrder(199.99m);

        _orderRepository
            .Setup(x => x.GetByIdAsync(order.Id))
            .ReturnsAsync(order);

        var service = CreateService();

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateCheckoutSessionAsync(
                order.UserId,
                new CreateCheckoutSessionRequest
                {
                    OrderId = order.Id
                }));

        // Assert
        Assert.Equal(
            "Minimum order value is $200 USD.",
            exception.Message);

        _paymentRepository.Verify(
            x => x.AddAsync(It.IsAny<Payment>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateCheckoutSessionAsync_ShouldRejectNonUsdOrder()
    {
        // Arrange
        var order = CreateOrder(300m);
        order.Currency = "INR";

        _orderRepository
            .Setup(x => x.GetByIdAsync(order.Id))
            .ReturnsAsync(order);

        var service = CreateService();

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateCheckoutSessionAsync(
                order.UserId,
                new CreateCheckoutSessionRequest
                {
                    OrderId = order.Id
                }));

        // Assert
        Assert.Equal(
            "Only USD payments are supported.",
            exception.Message);
    }

    [Fact]
    public async Task CreateCheckoutSessionAsync_ShouldRejectPaidOrder()
    {
        // Arrange
        var order = CreateOrder(300m);
        order.Status = "Paid";

        _orderRepository
            .Setup(x => x.GetByIdAsync(order.Id))
            .ReturnsAsync(order);

        var service = CreateService();

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateCheckoutSessionAsync(
                order.UserId,
                new CreateCheckoutSessionRequest
                {
                    OrderId = order.Id
                }));

        // Assert
        Assert.Equal(
            "Only pending orders can be paid.",
            exception.Message);
    }

    [Fact]
    public async Task CreateCheckoutSessionAsync_ShouldRejectOrderWithNoItems()
    {
        // Arrange
        var order = CreateOrder(300m);
        order.Items.Clear();

        _orderRepository
            .Setup(x => x.GetByIdAsync(order.Id))
            .ReturnsAsync(order);

        var service = CreateService();

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateCheckoutSessionAsync(
                order.UserId,
                new CreateCheckoutSessionRequest
                {
                    OrderId = order.Id
                }));

        // Assert
        Assert.Equal(
            "Order has no items.",
            exception.Message);
    }

    [Fact]
    public async Task CreateCheckoutSessionAsync_ShouldRejectUnauthorizedUser()
    {
        // Arrange
        var order = CreateOrder(300m);

        _orderRepository
            .Setup(x => x.GetByIdAsync(order.Id))
            .ReturnsAsync(order);

        var service = CreateService();

        // Act
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.CreateCheckoutSessionAsync(
                999,
                new CreateCheckoutSessionRequest
                {
                    OrderId = order.Id
                }));

        // Assert
        Assert.Equal(
            "You are not authorized to pay for this order.",
            exception.Message);
    }

    private static Order CreateOrder(decimal amount)
    {
        var order = new Order
        {
            Id = 100,
            UserId = 10,
            OrderNumber = "ORD-TEST-100",
            TotalAmount = amount,
            Currency = "USD",
            Status = "Pending"
        };

        order.Items.Add(
            new OrderItem
            {
                Id = 1,
                OrderId = order.Id,
                ProductVariantId = 1,
                ProductName = "Test Product",
                Strength = "500mg",
                PackSize = "10 Tablets",
                Quantity = 1,
                UnitPrice = amount
            });

        return order;
    }
}