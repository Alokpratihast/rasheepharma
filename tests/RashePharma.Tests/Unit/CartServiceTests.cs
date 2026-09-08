using Moq;
using RashePharma.Application.DTOs.Cart;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Services;
using RashePharma.Domain.Entities;

namespace RashePharma.Tests.Unit;

public class CartServiceTests
{
    // =========================================================
    // GetCartAsync
    // =========================================================

    [Fact]
    public async Task GetCartAsync_ShouldReturnExistingCart()
    {
        // Arrange
        var cartRepository = new Mock<ICartRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var cart = CreateCart();

        cartRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync(cart);

        var service = new CartService(
            cartRepository.Object,
            productRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.GetCartAsync(10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Single(result.Items);

        Assert.Equal(1, result.Items[0].Id);
        Assert.Equal(5, result.Items[0].ProductVariantId);
        Assert.Equal("MEDOFCIN-200", result.Items[0].ProductName);
        Assert.Equal("200 mg", result.Items[0].Strength);
        Assert.Equal("10 Tablets", result.Items[0].PackSize);
        Assert.Equal(2, result.Items[0].Quantity);
        Assert.Equal(100, result.Items[0].UnitPrice);
        Assert.Equal(200, result.Items[0].TotalPrice);
        Assert.Equal(200, result.TotalAmount);

        cartRepository.Verify(
            r => r.AddAsync(It.IsAny<Cart>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }

    [Fact]
    public async Task GetCartAsync_ShouldCreateCart_WhenCartDoesNotExist()
    {
        // Arrange
        var cartRepository = new Mock<ICartRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        cartRepository
            .SetupSequence(r => r.GetByUserIdAsync(10))
            .ReturnsAsync((Cart?)null)
            .ReturnsAsync(new Cart
            {
                Id = 1,
                UserId = 10,
                Items = new List<CartItem>()
            });

        cartRepository
            .Setup(r => r.AddAsync(It.IsAny<Cart>()))
            .Callback<Cart>(cart => cart.Id = 1)
            .Returns(Task.CompletedTask);

        var service = new CartService(
            cartRepository.Object,
            productRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.GetCartAsync(10);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalAmount);

        cartRepository.Verify(
            r => r.AddAsync(It.Is<Cart>(
                c => c.UserId == 10)),
            Times.Once);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task GetCartAsync_ShouldReturnEmptyItems_WhenCartHasNoItems()
    {
        // Arrange
        var cartRepository = new Mock<ICartRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var cart = CreateEmptyCart();

        cartRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync(cart);

        var service = new CartService(
            cartRepository.Object,
            productRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.GetCartAsync(10);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalAmount);
    }

    // =========================================================
    // AddToCartAsync
    // =========================================================

    [Fact]
    public async Task AddToCartAsync_ShouldAddNewItemSuccessfully()
    {
        // Arrange
        var cartRepository = new Mock<ICartRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var cart = CreateEmptyCart();

        var variant = CreateVariant(
            stockQuantity: 50,
            price: 100);

        var dto = new AddToCartDto
        {
            ProductVariantId = 5,
            Quantity = 2
        };

        productRepository
            .Setup(r => r.GetVariantByIdAsync(5))
            .ReturnsAsync(variant);

        cartRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync(cart);

        cartRepository
            .Setup(r => r.GetItemAsync(1, 5))
            .ReturnsAsync((CartItem?)null);

        cartRepository
            .Setup(r => r.AddItemAsync(It.IsAny<CartItem>()))
            .Callback<CartItem>(item =>
            {
                item.Id = 1;
                item.ProductVariant = variant;
                cart.Items.Add(item);
            })
            .Returns(Task.CompletedTask);

        unitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        var service = new CartService(
            cartRepository.Object,
            productRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.AddToCartAsync(10, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result.Items);

        Assert.Equal(5, result.Items[0].ProductVariantId);
        Assert.Equal(2, result.Items[0].Quantity);
        Assert.Equal(100, result.Items[0].UnitPrice);
        Assert.Equal(200, result.Items[0].TotalPrice);
        Assert.Equal(200, result.TotalAmount);

        cartRepository.Verify(
            r => r.AddItemAsync(It.Is<CartItem>(
                i => i.CartId == 1 &&
                     i.ProductVariantId == 5 &&
                     i.Quantity == 2 &&
                     i.UnitPrice == 100)),
            Times.Once);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task AddToCartAsync_ShouldIncreaseQuantity_WhenItemAlreadyExists()
    {
        // Arrange
        var cartRepository = new Mock<ICartRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var cart = CreateCart();
        var existingItem = cart.Items.First();

        var variant = CreateVariant(
            stockQuantity: 50,
            price: 100);

        var dto = new AddToCartDto
        {
            ProductVariantId = 5,
            Quantity = 3
        };

        productRepository
            .Setup(r => r.GetVariantByIdAsync(5))
            .ReturnsAsync(variant);

        cartRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync(cart);

        cartRepository
            .Setup(r => r.GetItemAsync(1, 5))
            .ReturnsAsync(existingItem);

        var service = new CartService(
            cartRepository.Object,
            productRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.AddToCartAsync(10, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(5, existingItem.Quantity);
        Assert.Equal(100, existingItem.UnitPrice);

        cartRepository.Verify(
            r => r.UpdateItemAsync(existingItem),
            Times.Once);

        cartRepository.Verify(
            r => r.AddItemAsync(It.IsAny<CartItem>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task AddToCartAsync_ShouldCreateCart_WhenCartDoesNotExist()
    {
        // Arrange
        var cartRepository = new Mock<ICartRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        Cart? createdCart = null;

        var variant = CreateVariant(
            stockQuantity: 50,
            price: 100);

        productRepository
            .Setup(r => r.GetVariantByIdAsync(5))
            .ReturnsAsync(variant);

        cartRepository
            .SetupSequence(r => r.GetByUserIdAsync(10))
            .ReturnsAsync((Cart?)null)
            .ReturnsAsync(() => createdCart);

        cartRepository
            .Setup(r => r.AddAsync(It.IsAny<Cart>()))
            .Callback<Cart>(cart =>
            {
                createdCart = cart;
                cart.Id = 1;
                cart.Items = new List<CartItem>();
            })
            .Returns(Task.CompletedTask);

        cartRepository
            .Setup(r => r.GetItemAsync(1, 5))
            .ReturnsAsync((CartItem?)null);

        cartRepository
            .Setup(r => r.AddItemAsync(It.IsAny<CartItem>()))
            .Callback<CartItem>(item =>
            {
                item.Id = 1;
                item.ProductVariant = variant;
                createdCart!.Items.Add(item);
            })
            .Returns(Task.CompletedTask);

        var dto = new AddToCartDto
        {
            ProductVariantId = 5,
            Quantity = 2
        };

        var service = new CartService(
            cartRepository.Object,
            productRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.AddToCartAsync(10, dto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Single(result.Items);
        Assert.Equal(5, result.Items[0].ProductVariantId);
        Assert.Equal(2, result.Items[0].Quantity);
        Assert.Equal(100, result.Items[0].UnitPrice);
        Assert.Equal(200, result.TotalAmount);

        cartRepository.Verify(
            r => r.AddAsync(It.Is<Cart>(
                c => c.UserId == 10)),
            Times.Once);

        cartRepository.Verify(
            r => r.AddItemAsync(It.Is<CartItem>(
                i => i.UnitPrice == 100)),
            Times.Once);
    }

    [Fact]
    public async Task AddToCartAsync_ShouldThrow_WhenQuantityIsZero()
    {
        // Arrange
        var cartRepository = new Mock<ICartRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = new CartService(
            cartRepository.Object,
            productRepository.Object,
            unitOfWork.Object);

        var dto = new AddToCartDto
        {
            ProductVariantId = 5,
            Quantity = 0
        };

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AddToCartAsync(10, dto));

        // Assert
        Assert.Equal(
            "Quantity must be greater than zero.",
            exception.Message);

        productRepository.Verify(
            r => r.GetVariantByIdAsync(It.IsAny<int>()),
            Times.Never);
    }

    [Fact]
    public async Task AddToCartAsync_ShouldThrow_WhenVariantDoesNotExist()
    {
        // Arrange
        var cartRepository = new Mock<ICartRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        productRepository
            .Setup(r => r.GetVariantByIdAsync(5))
            .ReturnsAsync((ProductVariant?)null);

        var service = new CartService(
            cartRepository.Object,
            productRepository.Object,
            unitOfWork.Object);

        var dto = new AddToCartDto
        {
            ProductVariantId = 5,
            Quantity = 2
        };

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AddToCartAsync(10, dto));

        // Assert
        Assert.Equal(
            "Product variant not found.",
            exception.Message);
    }

    [Fact]
    public async Task AddToCartAsync_ShouldThrow_WhenVariantIsInactive()
    {
        // Arrange
        var cartRepository = new Mock<ICartRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var variant = CreateVariant(
            stockQuantity: 50,
            price: 100);

        variant.IsActive = false;

        productRepository
            .Setup(r => r.GetVariantByIdAsync(5))
            .ReturnsAsync(variant);

        var service = new CartService(
            cartRepository.Object,
            productRepository.Object,
            unitOfWork.Object);

        var dto = new AddToCartDto
        {
            ProductVariantId = 5,
            Quantity = 2
        };

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AddToCartAsync(10, dto));

        // Assert
        Assert.Equal(
            "Product variant is not active.",
            exception.Message);
    }

    [Fact]
    public async Task AddToCartAsync_ShouldThrow_WhenVariantIsOutOfStock()
    {
        // Arrange
        var cartRepository = new Mock<ICartRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var variant = CreateVariant(
            stockQuantity: 0,
            price: 100);

        productRepository
            .Setup(r => r.GetVariantByIdAsync(5))
            .ReturnsAsync(variant);

        var service = new CartService(
            cartRepository.Object,
            productRepository.Object,
            unitOfWork.Object);

        var dto = new AddToCartDto
        {
            ProductVariantId = 5,
            Quantity = 2
        };

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AddToCartAsync(10, dto));

        // Assert
        Assert.Equal(
            "Product variant is out of stock.",
            exception.Message);
    }

    [Fact]
    public async Task AddToCartAsync_ShouldThrow_WhenQuantityExceedsStock()
    {
        // Arrange
        var cartRepository = new Mock<ICartRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var variant = CreateVariant(
            stockQuantity: 5,
            price: 100);

        productRepository
            .Setup(r => r.GetVariantByIdAsync(5))
            .ReturnsAsync(variant);

        cartRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync(CreateEmptyCart());

        cartRepository
            .Setup(r => r.GetItemAsync(1, 5))
            .ReturnsAsync((CartItem?)null);

        var service = new CartService(
            cartRepository.Object,
            productRepository.Object,
            unitOfWork.Object);

        var dto = new AddToCartDto
        {
            ProductVariantId = 5,
            Quantity = 6
        };

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AddToCartAsync(10, dto));

        // Assert
        Assert.Equal(
            "Insufficient stock.",
            exception.Message);

        cartRepository.Verify(
            r => r.AddItemAsync(It.IsAny<CartItem>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }

    [Fact]
    public async Task AddToCartAsync_ShouldThrow_WhenExistingQuantityExceedsStock()
    {
        // Arrange
        var cartRepository = new Mock<ICartRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var cart = CreateCart();
        var existingItem = cart.Items.First();

        var variant = CreateVariant(
            stockQuantity: 4,
            price: 100);

        productRepository
            .Setup(r => r.GetVariantByIdAsync(5))
            .ReturnsAsync(variant);

        cartRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync(cart);

        cartRepository
            .Setup(r => r.GetItemAsync(1, 5))
            .ReturnsAsync(existingItem);

        var service = new CartService(
            cartRepository.Object,
            productRepository.Object,
            unitOfWork.Object);

        var dto = new AddToCartDto
        {
            ProductVariantId = 5,
            Quantity = 3
        };

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.AddToCartAsync(10, dto));

        // Assert
        Assert.Equal(
            "Insufficient stock.",
            exception.Message);

        Assert.Equal(2, existingItem.Quantity);

        cartRepository.Verify(
            r => r.UpdateItemAsync(It.IsAny<CartItem>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }

    // =========================================================
    // UpdateItemAsync
    // =========================================================

    [Fact]
    public async Task UpdateItemAsync_ShouldUpdateQuantitySuccessfully()
    {
        // Arrange
        var cartRepository = new Mock<ICartRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var cart = CreateCart();
        var item = cart.Items.First();

        var variant = CreateVariant(
            stockQuantity: 50,
            price: 120);

        productRepository
            .Setup(r => r.GetVariantByIdAsync(5))
            .ReturnsAsync(variant);

        cartRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync(cart);

        cartRepository
            .Setup(r => r.GetItemAsync(1, 5))
            .ReturnsAsync(item);

        var service = new CartService(
            cartRepository.Object,
            productRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.UpdateItemAsync(10, 5, 7);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(7, item.Quantity);
        Assert.Equal(120, item.UnitPrice);

        Assert.Equal(840, result.Items[0].TotalPrice);
        Assert.Equal(840, result.TotalAmount);

        cartRepository.Verify(
            r => r.UpdateItemAsync(item),
            Times.Once);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task UpdateItemAsync_ShouldReturnNull_WhenCartDoesNotExist()
    {
        // Arrange
        var cartRepository = new Mock<ICartRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        cartRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync((Cart?)null);

        var service = new CartService(
            cartRepository.Object,
            productRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.UpdateItemAsync(10, 5, 3);

        // Assert
        Assert.Null(result);

        cartRepository.Verify(
            r => r.GetItemAsync(
                It.IsAny<int>(),
                It.IsAny<int>()),
            Times.Never);

        productRepository.Verify(
            r => r.GetVariantByIdAsync(It.IsAny<int>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }

    [Fact]
    public async Task UpdateItemAsync_ShouldReturnNull_WhenItemDoesNotExist()
    {
        // Arrange
        var cartRepository = new Mock<ICartRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var cart = CreateEmptyCart();

        cartRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync(cart);

        cartRepository
            .Setup(r => r.GetItemAsync(1, 5))
            .ReturnsAsync((CartItem?)null);

        var service = new CartService(
            cartRepository.Object,
            productRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.UpdateItemAsync(10, 5, 3);

        // Assert
        Assert.Null(result);

        cartRepository.Verify(
            r => r.UpdateItemAsync(It.IsAny<CartItem>()),
            Times.Never);

        productRepository.Verify(
            r => r.GetVariantByIdAsync(It.IsAny<int>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }

    [Fact]
    public async Task UpdateItemAsync_ShouldThrow_WhenQuantityIsZero()
    {
        // Arrange
        var cartRepository = new Mock<ICartRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var service = new CartService(
            cartRepository.Object,
            productRepository.Object,
            unitOfWork.Object);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateItemAsync(10, 5, 0));

        // Assert
        Assert.Equal(
            "Quantity must be greater than zero.",
            exception.Message);
    }

    [Fact]
    public async Task UpdateItemAsync_ShouldThrow_WhenVariantDoesNotExist()
    {
        // Arrange
        var cartRepository = new Mock<ICartRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var cart = CreateCart();
        var item = cart.Items.First();

        cartRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync(cart);

        cartRepository
            .Setup(r => r.GetItemAsync(1, 5))
            .ReturnsAsync(item);

        productRepository
            .Setup(r => r.GetVariantByIdAsync(5))
            .ReturnsAsync((ProductVariant?)null);

        var service = new CartService(
            cartRepository.Object,
            productRepository.Object,
            unitOfWork.Object);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateItemAsync(10, 5, 3));

        // Assert
        Assert.Equal(
            "Product variant not found.",
            exception.Message);
    }

    [Fact]
    public async Task UpdateItemAsync_ShouldThrow_WhenVariantIsInactive()
    {
        // Arrange
        var cartRepository = new Mock<ICartRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var cart = CreateCart();
        var item = cart.Items.First();

        var variant = CreateVariant(
            stockQuantity: 50,
            price: 120);

        variant.IsActive = false;

        cartRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync(cart);

        cartRepository
            .Setup(r => r.GetItemAsync(1, 5))
            .ReturnsAsync(item);

        productRepository
            .Setup(r => r.GetVariantByIdAsync(5))
            .ReturnsAsync(variant);

        var service = new CartService(
            cartRepository.Object,
            productRepository.Object,
            unitOfWork.Object);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateItemAsync(10, 5, 3));

        // Assert
        Assert.Equal(
            "Product variant is not active.",
            exception.Message);
    }

    [Fact]
    public async Task UpdateItemAsync_ShouldThrow_WhenQuantityExceedsStock()
    {
        // Arrange
        var cartRepository = new Mock<ICartRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var cart = CreateCart();
        var item = cart.Items.First();

        var variant = CreateVariant(
            stockQuantity: 5,
            price: 120);

        cartRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync(cart);

        cartRepository
            .Setup(r => r.GetItemAsync(1, 5))
            .ReturnsAsync(item);

        productRepository
            .Setup(r => r.GetVariantByIdAsync(5))
            .ReturnsAsync(variant);

        var service = new CartService(
            cartRepository.Object,
            productRepository.Object,
            unitOfWork.Object);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.UpdateItemAsync(10, 5, 6));

        // Assert
        Assert.Equal(
            "Insufficient stock.",
            exception.Message);

        Assert.Equal(2, item.Quantity);

        cartRepository.Verify(
            r => r.UpdateItemAsync(It.IsAny<CartItem>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }

    // =========================================================
    // RemoveItemAsync
    // =========================================================

    [Fact]
    public async Task RemoveItemAsync_ShouldRemoveItemSuccessfully()
    {
        // Arrange
        var cartRepository = new Mock<ICartRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var cart = CreateCart();
        var item = cart.Items.First();

        cartRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync(cart);

        cartRepository
            .Setup(r => r.GetItemAsync(1, 5))
            .ReturnsAsync(item);

        var service = new CartService(
            cartRepository.Object,
            productRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.RemoveItemAsync(10, 5);

        // Assert
        Assert.True(result);

        cartRepository.Verify(
            r => r.RemoveItemAsync(item),
            Times.Once);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task RemoveItemAsync_ShouldReturnFalse_WhenCartDoesNotExist()
    {
        // Arrange
        var cartRepository = new Mock<ICartRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        cartRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync((Cart?)null);

        var service = new CartService(
            cartRepository.Object,
            productRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.RemoveItemAsync(10, 5);

        // Assert
        Assert.False(result);

        cartRepository.Verify(
            r => r.GetItemAsync(
                It.IsAny<int>(),
                It.IsAny<int>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }

    [Fact]
    public async Task RemoveItemAsync_ShouldReturnFalse_WhenItemDoesNotExist()
    {
        // Arrange
        var cartRepository = new Mock<ICartRepository>();
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var cart = CreateEmptyCart();

        cartRepository
            .Setup(r => r.GetByUserIdAsync(10))
            .ReturnsAsync(cart);

        cartRepository
            .Setup(r => r.GetItemAsync(1, 5))
            .ReturnsAsync((CartItem?)null);

        var service = new CartService(
            cartRepository.Object,
            productRepository.Object,
            unitOfWork.Object);

        // Act
        var result = await service.RemoveItemAsync(10, 5);

        // Assert
        Assert.False(result);

        cartRepository.Verify(
            r => r.RemoveItemAsync(It.IsAny<CartItem>()),
            Times.Never);

        unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }

    // =========================================================
    // Helper Methods
    // =========================================================

    private static ProductVariant CreateVariant(
        int stockQuantity,
        decimal price)
    {
        return new ProductVariant
        {
            Id = 5,
            Strength = "200 mg",
            PackSize = "10 Tablets",
            Price = price,
            StockQuantity = stockQuantity,
            IsActive = true,
            Product = new Product
            {
                Id = 1,
                Name = "MEDOFCIN-200"
            }
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
                    UnitPrice = 100,
                    ProductVariant = new ProductVariant
                    {
                        Id = 5,
                        Strength = "200 mg",
                        PackSize = "10 Tablets",
                        Price = 100,
                        StockQuantity = 50,
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
}