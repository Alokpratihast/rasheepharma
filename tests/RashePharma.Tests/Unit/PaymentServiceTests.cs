using Microsoft.Extensions.Configuration;
using Moq;
using RashePharma.Application.DTOs.Payments;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Services;
using RashePharma.Domain.Entities;

namespace RashePharma.Tests.Unit;

public class PaymentServiceTests
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
    public async Task CreateCheckoutSessionAsync_ShouldThrow_WhenOrderDoesNotExist()
    {
        // Arrange
        _orderRepository
            .Setup(x => x.GetByIdAsync(999))
            .ReturnsAsync((Order?)null);

        var service = CreateService();

        var request = new CreateCheckoutSessionRequest
        {
            OrderId = 999
        };

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateCheckoutSessionAsync(1, request));

        // Assert
        Assert.Equal("Order not found.", exception.Message);

        _paymentRepository.Verify(
            x => x.AddAsync(It.IsAny<Payment>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateCheckoutSessionAsync_ShouldThrow_WhenUserDoesNotOwnOrder()
    {
        // Arrange
        var order = CreateOrder();

        _orderRepository
            .Setup(x => x.GetByIdAsync(order.Id))
            .ReturnsAsync(order);

        var service = CreateService();

        var request = new CreateCheckoutSessionRequest
        {
            OrderId = order.Id
        };

        // Act
        var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => service.CreateCheckoutSessionAsync(999, request));

        // Assert
        Assert.Equal(
            "You are not authorized to pay for this order.",
            exception.Message);
    }

    [Fact]
    public async Task CreateCheckoutSessionAsync_ShouldThrow_WhenOrderIsNotPending()
    {
        // Arrange
        var order = CreateOrder();
        order.Status = "Paid";

        _orderRepository
            .Setup(x => x.GetByIdAsync(order.Id))
            .ReturnsAsync(order);

        var service = CreateService();

        var request = new CreateCheckoutSessionRequest
        {
            OrderId = order.Id
        };

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateCheckoutSessionAsync(1, request));

        // Assert
        Assert.Equal(
            "Only pending orders can be paid.",
            exception.Message);
    }

    [Fact]
    public async Task CreateCheckoutSessionAsync_ShouldThrow_WhenCurrencyIsNotUsd()
    {
        // Arrange
        var order = CreateOrder();
        order.Currency = "EUR";

        _orderRepository
            .Setup(x => x.GetByIdAsync(order.Id))
            .ReturnsAsync(order);

        var service = CreateService();

        var request = new CreateCheckoutSessionRequest
        {
            OrderId = order.Id
        };

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateCheckoutSessionAsync(1, request));

        // Assert
        Assert.Equal(
            "Only USD payments are supported.",
            exception.Message);
    }

    [Fact]
    public async Task CreateCheckoutSessionAsync_ShouldThrow_WhenAmountIsBelow200()
    {
        // Arrange
        var order = CreateOrder();
        order.TotalAmount = 199.99m;

        _orderRepository
            .Setup(x => x.GetByIdAsync(order.Id))
            .ReturnsAsync(order);

        var service = CreateService();

        var request = new CreateCheckoutSessionRequest
        {
            OrderId = order.Id
        };

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateCheckoutSessionAsync(1, request));

        // Assert
        Assert.Equal(
            "Minimum order value is $200 USD.",
            exception.Message);
    }

    [Fact]
    public async Task CreateCheckoutSessionAsync_ShouldThrow_WhenOrderHasNoItems()
    {
        // Arrange
        var order = CreateOrder();
        order.Items.Clear();

        _orderRepository
            .Setup(x => x.GetByIdAsync(order.Id))
            .ReturnsAsync(order);

        var service = CreateService();

        var request = new CreateCheckoutSessionRequest
        {
            OrderId = order.Id
        };

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateCheckoutSessionAsync(1, request));

        // Assert
        Assert.Equal(
            "Order has no items.",
            exception.Message);
    }

    private static Order CreateOrder()
    {
        var order = new Order
        {
            Id = 1,
            UserId = 1,
            OrderNumber = "ORD-TEST-001",
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
                ProductName = "Test Product",
                Strength = "500mg",
                PackSize = "10 Tablets",
                Quantity = 2,
                UnitPrice = 150m
            });

        return order;
    }
}