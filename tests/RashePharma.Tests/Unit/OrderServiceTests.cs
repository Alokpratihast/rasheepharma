using Moq;
using RashePharma.Application.DTOs.Orders;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Services;
using RashePharma.Domain.Entities;

namespace RashePharma.Tests.Unit;

public class OrderServiceTests
{
    // =========================================================
    // GetMyOrdersAsync
    // =========================================================

    [Fact]
    public async Task GetMyOrdersAsync_ShouldReturnOrders()
    {
        var orderRepository = new Mock<IOrderRepository>();
        var cartRepository = new Mock<ICartRepository>();
        var addressRepository = new Mock<IAddressRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var orders = new List<Order>
        {
            new Order
            {
                Id = 1,
                UserId = 10,
                OrderNumber = "ORD-001",
                TotalAmount = 500,
                Currency = "INR",
                Status = "Pending"
            },
            new Order
            {
                Id = 2,
                UserId = 10,
                OrderNumber = "ORD-002",
                TotalAmount = 750,
                Currency = "INR",
                Status = "Shipped"
            }
        };

        orderRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync(orders);

        var service = CreateService(
            orderRepository,
            cartRepository,
            addressRepository,
            productRepository,
            unitOfWork);

        var result = await service.GetMyOrdersAsync(10);

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Equal("ORD-001", result[0].OrderNumber);
        Assert.Equal(500, result[0].TotalAmount);
        Assert.Equal("Pending", result[0].Status);
        Assert.Equal("ORD-002", result[1].OrderNumber);
    }

    [Fact]
    public async Task GetMyOrdersAsync_ShouldReturnEmpty_WhenNoOrders()
    {
        var orderRepository = new Mock<IOrderRepository>();
        var cartRepository = new Mock<ICartRepository>();
        var addressRepository = new Mock<IAddressRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        orderRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync(new List<Order>());

        var service = CreateService(
            orderRepository,
            cartRepository,
            addressRepository,
            productRepository,
            unitOfWork);

        var result = await service.GetMyOrdersAsync(10);

        Assert.NotNull(result);
        Assert.Empty(result);
    }

    // =========================================================
    // GetByIdAsync
    // =========================================================

    [Fact]
    public async Task GetByIdAsync_ShouldReturnOrder_WhenOrderBelongsToUser()
    {
        var orderRepository = new Mock<IOrderRepository>();
        var cartRepository = new Mock<ICartRepository>();
        var addressRepository = new Mock<IAddressRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var order = CreateOrder(1, 10);

        orderRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(order);

        var service = CreateService(
            orderRepository,
            cartRepository,
            addressRepository,
            productRepository,
            unitOfWork);

        var result = await service.GetByIdAsync(1, 10);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("ORD-001", result.OrderNumber);
        Assert.Equal("INR", result.Currency);
        Assert.Equal("Pending", result.Status);
        Assert.Single(result.Items);
        Assert.Equal(250, result.Items[0].TotalPrice);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenOrderDoesNotExist()
    {
        var orderRepository = new Mock<IOrderRepository>();
        var cartRepository = new Mock<ICartRepository>();
        var addressRepository = new Mock<IAddressRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        orderRepository
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Order?)null);

        var service = CreateService(
            orderRepository,
            cartRepository,
            addressRepository,
            productRepository,
            unitOfWork);

        var result = await service.GetByIdAsync(999, 10);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenOrderBelongsToAnotherUser()
    {
        var orderRepository = new Mock<IOrderRepository>();
        var cartRepository = new Mock<ICartRepository>();
        var addressRepository = new Mock<IAddressRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var order = CreateOrder(1, 20);

        orderRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(order);

        var service = CreateService(
            orderRepository,
            cartRepository,
            addressRepository,
            productRepository,
            unitOfWork);

        var result = await service.GetByIdAsync(1, 10);

        Assert.Null(result);
    }

    // =========================================================
    // GetByOrderNumberAsync
    // =========================================================

    [Fact]
    public async Task GetByOrderNumberAsync_ShouldReturnOrder_WhenOrderBelongsToUser()
    {
        var orderRepository = new Mock<IOrderRepository>();
        var cartRepository = new Mock<ICartRepository>();
        var addressRepository = new Mock<IAddressRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var order = CreateOrder(1, 10);

        orderRepository
            .Setup(r => r.GetByOrderNumberAsync("ORD-001"))
            .ReturnsAsync(order);

        var service = CreateService(
            orderRepository,
            cartRepository,
            addressRepository,
            productRepository,
            unitOfWork);

        var result = await service.GetByOrderNumberAsync("ORD-001", 10);

        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("ORD-001", result.OrderNumber);
        Assert.Single(result.Items);
    }

    [Fact]
    public async Task GetByOrderNumberAsync_ShouldReturnNull_WhenOrderDoesNotExist()
    {
        var orderRepository = new Mock<IOrderRepository>();
        var cartRepository = new Mock<ICartRepository>();
        var addressRepository = new Mock<IAddressRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        orderRepository
            .Setup(r => r.GetByOrderNumberAsync("ORD-999"))
            .ReturnsAsync((Order?)null);

        var service = CreateService(
            orderRepository,
            cartRepository,
            addressRepository,
            productRepository,
            unitOfWork);

        var result = await service.GetByOrderNumberAsync("ORD-999", 10);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetByOrderNumberAsync_ShouldReturnNull_WhenOrderBelongsToAnotherUser()
    {
        var orderRepository = new Mock<IOrderRepository>();
        var cartRepository = new Mock<ICartRepository>();
        var addressRepository = new Mock<IAddressRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var order = CreateOrder(1, 20);

        orderRepository
            .Setup(r => r.GetByOrderNumberAsync("ORD-001"))
            .ReturnsAsync(order);

        var service = CreateService(
            orderRepository,
            cartRepository,
            addressRepository,
            productRepository,
            unitOfWork);

        var result = await service.GetByOrderNumberAsync("ORD-001", 10);

        Assert.Null(result);
    }

    // =========================================================
    // CreateAsync
    // =========================================================

    [Fact]
    public async Task CreateAsync_ShouldCreateOrderSuccessfully()
    {
        var orderRepository = new Mock<IOrderRepository>();
        var cartRepository = new Mock<ICartRepository>();
        var addressRepository = new Mock<IAddressRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var transaction = CreateTransactionMock();

        var cart = CreateCart();
        var address = CreateAddress(1, 10);

        cartRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync(cart);

        addressRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(address);

        productRepository
            .Setup(r => r.GetVariantByIdAsync(5))
            .ReturnsAsync(cart.Items.First().ProductVariant);

        unitOfWork
            .Setup(u => u.BeginTransactionAsync())
            .ReturnsAsync(transaction.Object);

        Order? createdOrder = null;

        orderRepository
            .Setup(r => r.AddAsync(It.IsAny<Order>()))
            .Callback<Order>(order =>
            {
                createdOrder = order;
                order.Id = 100;
            })
            .Returns(Task.CompletedTask);

        unitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        var service = CreateService(
            orderRepository,
            cartRepository,
            addressRepository,
            productRepository,
            unitOfWork);

        var dto = new CreateOrderDto
        {
            AddressId = 1
        };

        var result = await service.CreateAsync(10, dto);

        Assert.NotNull(result);
        Assert.Equal(100, result.Id);
        Assert.StartsWith("ORD-", result.OrderNumber);
        Assert.Equal("INR", result.Currency);
        Assert.Equal("Pending", result.Status);
        Assert.Equal(250, result.TotalAmount);

        Assert.Equal("123 Main Street", result.ShippingAddressLine1);
        Assert.Equal("Bengaluru", result.ShippingCity);
        Assert.Equal("Karnataka", result.ShippingState);
        Assert.Equal("560001", result.ShippingPostalCode);
        Assert.Equal("India", result.ShippingCountry);

        Assert.Single(result.Items);
        Assert.Equal(5, result.Items[0].ProductVariantId);
        Assert.Equal("MEDOFCIN-200", result.Items[0].ProductName);
        Assert.Equal("200 mg", result.Items[0].Strength);
        Assert.Equal("10 Tablets", result.Items[0].PackSize);
        Assert.Equal(2, result.Items[0].Quantity);
        Assert.Equal(125, result.Items[0].UnitPrice);
        Assert.Equal(250, result.Items[0].TotalPrice);

        Assert.NotNull(createdOrder);
        Assert.Equal(250, createdOrder!.TotalAmount);

        orderRepository.Verify(
            r => r.AddAsync(It.IsAny<Order>()),
            Times.Once);

        orderRepository.Verify(
            r => r.AddStatusHistoryAsync(
                It.Is<OrderStatusHistory>(h =>
                    h.OrderId == 100 &&
                    h.Status == "Pending" &&
                    h.Comment == "Order created.")),
            Times.Once);

        cartRepository.Verify(
            r => r.ClearItemsAsync(cart.Id),
            Times.Once);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Exactly(2));

        transaction.Verify(
            t => t.CommitAsync(),
            Times.Once);

        transaction.Verify(
            t => t.RollbackAsync(),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenCartDoesNotExist()
    {
        var orderRepository = new Mock<IOrderRepository>();
        var cartRepository = new Mock<ICartRepository>();
        var addressRepository = new Mock<IAddressRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        cartRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync((Cart?)null);

        var service = CreateService(
            orderRepository,
            cartRepository,
            addressRepository,
            productRepository,
            unitOfWork);

        var dto = new CreateOrderDto
        {
            AddressId = 1
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(10, dto));

        Assert.Equal("Cart is empty.", exception.Message);

        orderRepository.Verify(
            r => r.AddAsync(It.IsAny<Order>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);

        unitOfWork.Verify(
            u => u.BeginTransactionAsync(),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenCartIsEmpty()
    {
        var orderRepository = new Mock<IOrderRepository>();
        var cartRepository = new Mock<ICartRepository>();
        var addressRepository = new Mock<IAddressRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        cartRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync(CreateEmptyCart());

        var service = CreateService(
            orderRepository,
            cartRepository,
            addressRepository,
            productRepository,
            unitOfWork);

        var dto = new CreateOrderDto
        {
            AddressId = 1
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(10, dto));

        Assert.Equal("Cart is empty.", exception.Message);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenShippingAddressDoesNotExist()
    {
        var orderRepository = new Mock<IOrderRepository>();
        var cartRepository = new Mock<ICartRepository>();
        var addressRepository = new Mock<IAddressRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        cartRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync(CreateCart());

        addressRepository
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Address?)null);

        var service = CreateService(
            orderRepository,
            cartRepository,
            addressRepository,
            productRepository,
            unitOfWork);

        var dto = new CreateOrderDto
        {
            AddressId = 999
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(10, dto));

        Assert.Equal("Invalid shipping address.", exception.Message);

        orderRepository.Verify(
            r => r.AddAsync(It.IsAny<Order>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.BeginTransactionAsync(),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenShippingAddressBelongsToAnotherUser()
    {
        var orderRepository = new Mock<IOrderRepository>();
        var cartRepository = new Mock<ICartRepository>();
        var addressRepository = new Mock<IAddressRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        cartRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync(CreateCart());

        addressRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(CreateAddress(1, 20));

        var service = CreateService(
            orderRepository,
            cartRepository,
            addressRepository,
            productRepository,
            unitOfWork);

        var dto = new CreateOrderDto
        {
            AddressId = 1
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(10, dto));

        Assert.Equal("Invalid shipping address.", exception.Message);

        orderRepository.Verify(
            r => r.AddAsync(It.IsAny<Order>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.BeginTransactionAsync(),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenProductVariantDoesNotExist()
    {
        var orderRepository = new Mock<IOrderRepository>();
        var cartRepository = new Mock<ICartRepository>();
        var addressRepository = new Mock<IAddressRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        cartRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync(CreateCart());

        addressRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(CreateAddress(1, 10));

        productRepository
            .Setup(r => r.GetVariantByIdAsync(5))
            .ReturnsAsync((ProductVariant?)null);

        var service = CreateService(
            orderRepository,
            cartRepository,
            addressRepository,
            productRepository,
            unitOfWork);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(
                10,
                new CreateOrderDto { AddressId = 1 }));

        Assert.Equal(
            "Product variant 5 not found.",
            exception.Message);

        orderRepository.Verify(
            r => r.AddAsync(It.IsAny<Order>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.BeginTransactionAsync(),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenProductVariantIsInactive()
    {
        var orderRepository = new Mock<IOrderRepository>();
        var cartRepository = new Mock<ICartRepository>();
        var addressRepository = new Mock<IAddressRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var variant = CreateVariant(
            id: 5,
            price: 125,
            stockQuantity: 10,
            isActive: false);

        cartRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync(CreateCart());

        addressRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(CreateAddress(1, 10));

        productRepository
            .Setup(r => r.GetVariantByIdAsync(5))
            .ReturnsAsync(variant);

        var service = CreateService(
            orderRepository,
            cartRepository,
            addressRepository,
            productRepository,
            unitOfWork);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(
                10,
                new CreateOrderDto { AddressId = 1 }));

        Assert.Equal(
            "Product variant 5 is not active.",
            exception.Message);

        orderRepository.Verify(
            r => r.AddAsync(It.IsAny<Order>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.BeginTransactionAsync(),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldThrow_WhenStockIsInsufficient()
    {
        var orderRepository = new Mock<IOrderRepository>();
        var cartRepository = new Mock<ICartRepository>();
        var addressRepository = new Mock<IAddressRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var variant = CreateVariant(
            id: 5,
            price: 125,
            stockQuantity: 1,
            isActive: true);

        cartRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync(CreateCart());

        addressRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(CreateAddress(1, 10));

        productRepository
            .Setup(r => r.GetVariantByIdAsync(5))
            .ReturnsAsync(variant);

        var service = CreateService(
            orderRepository,
            cartRepository,
            addressRepository,
            productRepository,
            unitOfWork);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(
                10,
                new CreateOrderDto { AddressId = 1 }));

        Assert.Equal(
            "Insufficient stock for MEDOFCIN-200.",
            exception.Message);

        orderRepository.Verify(
            r => r.AddAsync(It.IsAny<Order>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.BeginTransactionAsync(),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_ShouldUseLatestDatabasePrice()
    {
        var orderRepository = new Mock<IOrderRepository>();
        var cartRepository = new Mock<ICartRepository>();
        var addressRepository = new Mock<IAddressRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var transaction = CreateTransactionMock();

        var cart = CreateCart();

        // Cart contains old price = 125
        cart.Items.First().UnitPrice = 125;

        // DB contains latest price = 150
        var latestVariant = CreateVariant(
            id: 5,
            price: 150,
            stockQuantity: 10,
            isActive: true);

        cartRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync(cart);

        addressRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(CreateAddress(1, 10));

        productRepository
            .Setup(r => r.GetVariantByIdAsync(5))
            .ReturnsAsync(latestVariant);

        unitOfWork
            .Setup(u => u.BeginTransactionAsync())
            .ReturnsAsync(transaction.Object);

        orderRepository
            .Setup(r => r.AddAsync(It.IsAny<Order>()))
            .Callback<Order>(order => order.Id = 100)
            .Returns(Task.CompletedTask);

        unitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        var service = CreateService(
            orderRepository,
            cartRepository,
            addressRepository,
            productRepository,
            unitOfWork);

        var result = await service.CreateAsync(
            10,
            new CreateOrderDto { AddressId = 1 });

        Assert.Equal(150, result.Items[0].UnitPrice);
        Assert.Equal(300, result.TotalAmount);
        Assert.Equal(300, result.Items[0].TotalPrice);
    }

    [Fact]
    public async Task CreateAsync_ShouldCalculateTotalForMultipleItems()
    {
        var orderRepository = new Mock<IOrderRepository>();
        var cartRepository = new Mock<ICartRepository>();
        var addressRepository = new Mock<IAddressRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var transaction = CreateTransactionMock();

        var cart = CreateCart();

        var secondVariant = new ProductVariant
        {
            Id = 6,
            Strength = "500 mg",
            PackSize = "10 Tablets",
            Price = 50,
            StockQuantity = 10,
            IsActive = true,
            Product = new Product
            {
                Id = 2,
                Name = "TEST-PRODUCT"
            }
        };

        cart.Items.Add(new CartItem
        {
            Id = 2,
            CartId = 1,
            ProductVariantId = 6,
            Quantity = 3,
            UnitPrice = 50,
            ProductVariant = secondVariant
        });

        cartRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync(cart);

        addressRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(CreateAddress(1, 10));

        productRepository
            .Setup(r => r.GetVariantByIdAsync(5))
            .ReturnsAsync(cart.Items.First().ProductVariant);

        productRepository
            .Setup(r => r.GetVariantByIdAsync(6))
            .ReturnsAsync(secondVariant);

        unitOfWork
            .Setup(u => u.BeginTransactionAsync())
            .ReturnsAsync(transaction.Object);

        orderRepository
            .Setup(r => r.AddAsync(It.IsAny<Order>()))
            .Callback<Order>(order => order.Id = 101)
            .Returns(Task.CompletedTask);

        unitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        var service = CreateService(
            orderRepository,
            cartRepository,
            addressRepository,
            productRepository,
            unitOfWork);

        var result = await service.CreateAsync(
            10,
            new CreateOrderDto { AddressId = 1 });

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(400, result.TotalAmount);
        Assert.Equal(250, result.Items[0].TotalPrice);
        Assert.Equal(150, result.Items[1].TotalPrice);
    }

    [Fact]
    public async Task CreateAsync_ShouldClearCartAfterSuccessfulOrder()
    {
        var orderRepository = new Mock<IOrderRepository>();
        var cartRepository = new Mock<ICartRepository>();
        var addressRepository = new Mock<IAddressRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var transaction = CreateTransactionMock();

        var cart = CreateCart();

        cartRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync(cart);

        addressRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(CreateAddress(1, 10));

        productRepository
            .Setup(r => r.GetVariantByIdAsync(5))
            .ReturnsAsync(cart.Items.First().ProductVariant);

        unitOfWork
            .Setup(u => u.BeginTransactionAsync())
            .ReturnsAsync(transaction.Object);

        orderRepository
            .Setup(r => r.AddAsync(It.IsAny<Order>()))
            .Callback<Order>(order => order.Id = 100)
            .Returns(Task.CompletedTask);

        unitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        var service = CreateService(
            orderRepository,
            cartRepository,
            addressRepository,
            productRepository,
            unitOfWork);

        await service.CreateAsync(
            10,
            new CreateOrderDto { AddressId = 1 });

        cartRepository.Verify(
            r => r.ClearItemsAsync(cart.Id),
            Times.Once);

        transaction.Verify(
            t => t.CommitAsync(),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ShouldCreatePendingStatusHistory()
    {
        var orderRepository = new Mock<IOrderRepository>();
        var cartRepository = new Mock<ICartRepository>();
        var addressRepository = new Mock<IAddressRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var transaction = CreateTransactionMock();

        var cart = CreateCart();

        cartRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync(cart);

        addressRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(CreateAddress(1, 10));

        productRepository
            .Setup(r => r.GetVariantByIdAsync(5))
            .ReturnsAsync(cart.Items.First().ProductVariant);

        unitOfWork
            .Setup(u => u.BeginTransactionAsync())
            .ReturnsAsync(transaction.Object);

        orderRepository
            .Setup(r => r.AddAsync(It.IsAny<Order>()))
            .Callback<Order>(order => order.Id = 100)
            .Returns(Task.CompletedTask);

        unitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        OrderStatusHistory? createdHistory = null;

        orderRepository
            .Setup(r => r.AddStatusHistoryAsync(
                It.IsAny<OrderStatusHistory>()))
            .Callback<OrderStatusHistory>(history =>
            {
                createdHistory = history;
            })
            .Returns(Task.CompletedTask);

        var service = CreateService(
            orderRepository,
            cartRepository,
            addressRepository,
            productRepository,
            unitOfWork);

        await service.CreateAsync(
            10,
            new CreateOrderDto { AddressId = 1 });

        Assert.NotNull(createdHistory);
        Assert.Equal(100, createdHistory!.OrderId);
        Assert.Equal("Pending", createdHistory.Status);
        Assert.Equal("Order created.", createdHistory.Comment);
    }

    [Fact]
    public async Task CreateAsync_ShouldRollback_WhenOperationFailsInsideTransaction()
    {
        var orderRepository = new Mock<IOrderRepository>();
        var cartRepository = new Mock<ICartRepository>();
        var addressRepository = new Mock<IAddressRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();
        var transaction = CreateTransactionMock();

        var cart = CreateCart();

        cartRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync(cart);

        addressRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(CreateAddress(1, 10));

        productRepository
            .Setup(r => r.GetVariantByIdAsync(5))
            .ReturnsAsync(cart.Items.First().ProductVariant);

        unitOfWork
            .Setup(u => u.BeginTransactionAsync())
            .ReturnsAsync(transaction.Object);

        orderRepository
            .Setup(r => r.AddAsync(It.IsAny<Order>()))
            .ThrowsAsync(
                new InvalidOperationException(
                    "Database failure."));

        var service = CreateService(
            orderRepository,
            cartRepository,
            addressRepository,
            productRepository,
            unitOfWork);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateAsync(
                10,
                new CreateOrderDto { AddressId = 1 }));

        Assert.Equal(
            "Database failure.",
            exception.Message);

        transaction.Verify(
            t => t.RollbackAsync(),
            Times.Once);

        transaction.Verify(
            t => t.CommitAsync(),
            Times.Never);

        cartRepository.Verify(
            r => r.ClearItemsAsync(It.IsAny<int>()),
            Times.Never);
    }

    // =========================================================
    // UpdateStatusAsync
    // =========================================================

    [Fact]
    public async Task UpdateStatusAsync_ShouldUpdateStatusAndCreateHistory()
    {
        var orderRepository = new Mock<IOrderRepository>();
        var cartRepository = new Mock<ICartRepository>();
        var addressRepository = new Mock<IAddressRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var order = CreateOrder(1, 10);

        orderRepository
            .Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(order);

        OrderStatusHistory? createdHistory = null;

        orderRepository
            .Setup(r => r.AddStatusHistoryAsync(
                It.IsAny<OrderStatusHistory>()))
            .Callback<OrderStatusHistory>(history =>
            {
                createdHistory = history;
                history.Id = 5;
            })
            .Returns(Task.CompletedTask);

        unitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        var service = CreateService(
            orderRepository,
            cartRepository,
            addressRepository,
            productRepository,
            unitOfWork);

        var dto = new UpdateOrderStatusDto
        {
            Status = "Shipped",
            Comment = "Order dispatched."
        };

        var result = await service.UpdateStatusAsync(1, dto);

        Assert.True(result);
        Assert.Equal("Shipped", order.Status);
        Assert.NotNull(createdHistory);
        Assert.Equal(1, createdHistory!.OrderId);
        Assert.Equal("Shipped", createdHistory.Status);
        Assert.Equal("Order dispatched.", createdHistory.Comment);

        orderRepository.Verify(
            r => r.UpdateAsync(order),
            Times.Once);

        orderRepository.Verify(
            r => r.AddStatusHistoryAsync(
                It.IsAny<OrderStatusHistory>()),
            Times.Once);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task UpdateStatusAsync_ShouldReturnFalse_WhenOrderDoesNotExist()
    {
        var orderRepository = new Mock<IOrderRepository>();
        var cartRepository = new Mock<ICartRepository>();
        var addressRepository = new Mock<IAddressRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        orderRepository
            .Setup(r => r.GetByIdAsync(999))
            .ReturnsAsync((Order?)null);

        var service = CreateService(
            orderRepository,
            cartRepository,
            addressRepository,
            productRepository,
            unitOfWork);

        var dto = new UpdateOrderStatusDto
        {
            Status = "Shipped",
            Comment = "Order dispatched."
        };

        var result = await service.UpdateStatusAsync(999, dto);

        Assert.False(result);

        orderRepository.Verify(
            r => r.UpdateAsync(It.IsAny<Order>()),
            Times.Never);

        orderRepository.Verify(
            r => r.AddStatusHistoryAsync(
                It.IsAny<OrderStatusHistory>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }

    // =========================================================
    // Helpers
    // =========================================================

    private static OrderService CreateService(
        Mock<IOrderRepository> orderRepository,
        Mock<ICartRepository> cartRepository,
        Mock<IAddressRepository> addressRepository,
        Mock<IProductRepository> productRepository,
        Mock<IUnitOfWork> unitOfWork)
    {
        return new OrderService(
            orderRepository.Object,
            cartRepository.Object,
            addressRepository.Object,
            productRepository.Object,
            unitOfWork.Object);
    }

    private static Mock<ITransaction> CreateTransactionMock()
    {
        var transaction = new Mock<ITransaction>();

        transaction
            .Setup(t => t.CommitAsync())
            .Returns(Task.CompletedTask);

        transaction
            .Setup(t => t.RollbackAsync())
            .Returns(Task.CompletedTask);

        transaction
            .Setup(t => t.DisposeAsync())
            .Returns(ValueTask.CompletedTask);

        return transaction;
    }

    private static Order CreateOrder(int id, int userId)
    {
        return new Order
        {
            Id = id,
            UserId = userId,
            OrderNumber = "ORD-001",
            TotalAmount = 250,
            Currency = "INR",
            Status = "Pending",
            ShippingAddressLine1 = "123 Main Street",
            ShippingAddressLine2 = "Apartment 10",
            ShippingCity = "Bengaluru",
            ShippingState = "Karnataka",
            ShippingPostalCode = "560001",
            ShippingCountry = "India",

            Items = new List<OrderItem>
            {
                new OrderItem
                {
                    Id = 1,
                    ProductVariantId = 5,
                    ProductName = "MEDOFCIN-200",
                    Strength = "200 mg",
                    PackSize = "10 Tablets",
                    Quantity = 2,
                    UnitPrice = 125
                }
            },

            StatusHistory = new List<OrderStatusHistory>()
        };
    }

    private static Cart CreateEmptyCart()
    {
        return new Cart
        {
            Id = 1,
            UserId = 10,
            Items = new List<CartItem>()
        };
    }

    private static Cart CreateCart()
    {
        return new Cart
        {
            Id = 1,
            UserId = 10,

            Items = new List<CartItem>
            {
                new CartItem
                {
                    Id = 1,
                    CartId = 1,
                    ProductVariantId = 5,
                    Quantity = 2,
                    UnitPrice = 125,

                    ProductVariant = new ProductVariant
                    {
                        Id = 5,
                        Strength = "200 mg",
                        PackSize = "10 Tablets",
                        Price = 125,
                        StockQuantity = 10,
                        IsActive = true,

                        Product = new Product
                        {
                            Id = 1,
                            Name = "MEDOFCIN-200"
                        }
                    }
                }
            }
        };
    }

    private static ProductVariant CreateVariant(
        int id,
        decimal price,
        int stockQuantity,
        bool isActive)
    {
        return new ProductVariant
        {
            Id = id,
            Strength = "200 mg",
            PackSize = "10 Tablets",
            Price = price,
            StockQuantity = stockQuantity,
            IsActive = isActive,

            Product = new Product
            {
                Id = 1,
                Name = "MEDOFCIN-200"
            }
        };
    }

    private static Address CreateAddress(
        int id,
        int userId)
    {
        return new Address
        {
            Id = id,
            UserId = userId,
            AddressLine1 = "123 Main Street",
            AddressLine2 = "Apartment 10",
            City = "Bengaluru",
            State = "Karnataka",
            PostalCode = "560001",
            Country = "India",
            AddressType = "Home",
            IsDefault = true
        };
    }
}