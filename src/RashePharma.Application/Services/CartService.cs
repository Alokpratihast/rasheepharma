using RashePharma.Application.DTOs.Cart;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Domain.Entities;

namespace RashePharma.Application.Services;

public class CartService : ICartService
{
    private readonly ICartRepository _cartRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CartService(
        ICartRepository cartRepository,
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _cartRepository = cartRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<CartDto> GetCartAsync(int userId)
    {
        var cart = await _cartRepository.GetByUserIdAsync(userId);

        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId
            };

            await _cartRepository.AddAsync(cart);
            await _unitOfWork.SaveChangesAsync();

            // Reload so the cart has all required navigation data.
            var createdCart =
                await _cartRepository.GetByUserIdAsync(userId);

            if (createdCart == null)
                throw new InvalidOperationException(
                    "Cart could not be loaded after creation.");

            cart = createdCart;
        }

        return MapToDto(cart);
    }

    public async Task<CartDto> AddToCartAsync(
        int userId,
        AddToCartDto dto)
    {
        if (dto.Quantity <= 0)
            throw new InvalidOperationException(
                "Quantity must be greater than zero.");

        var variant = await _productRepository
            .GetVariantByIdAsync(dto.ProductVariantId);

        if (variant == null)
            throw new InvalidOperationException(
                "Product variant not found.");

        if (!variant.IsActive)
            throw new InvalidOperationException(
                "Product variant is not active.");

        if (variant.StockQuantity <= 0)
            throw new InvalidOperationException(
                "Product variant is out of stock.");

        var cart = await _cartRepository
            .GetByUserIdAsync(userId);

        if (cart == null)
        {
            cart = new Cart
            {
                UserId = userId
            };

            await _cartRepository.AddAsync(cart);
            await _unitOfWork.SaveChangesAsync();
        }

        var existingItem =
            await _cartRepository.GetItemAsync(
                cart.Id,
                dto.ProductVariantId);

        if (existingItem != null)
        {
            var newQuantity =
                existingItem.Quantity + dto.Quantity;

            if (newQuantity > variant.StockQuantity)
                throw new InvalidOperationException(
                    "Insufficient stock.");

            existingItem.Quantity = newQuantity;

            // Always use the price from the database.
            existingItem.UnitPrice = variant.Price;

            await _cartRepository
                .UpdateItemAsync(existingItem);
        }
        else
        {
            if (dto.Quantity > variant.StockQuantity)
                throw new InvalidOperationException(
                    "Insufficient stock.");

            var item = new CartItem
            {
                CartId = cart.Id,
                ProductVariantId = dto.ProductVariantId,
                Quantity = dto.Quantity,

                // Never trust price from client.
                UnitPrice = variant.Price
            };

            await _cartRepository.AddItemAsync(item);
        }

        await _unitOfWork.SaveChangesAsync();

        var updatedCart =
            await _cartRepository.GetByUserIdAsync(userId);

        if (updatedCart == null)
            throw new InvalidOperationException(
                "Cart could not be loaded after update.");

        return MapToDto(updatedCart);
    }

    public async Task<CartDto?> UpdateItemAsync(
        int userId,
        int productVariantId,
        int quantity)
    {
        if (quantity <= 0)
            throw new InvalidOperationException(
                "Quantity must be greater than zero.");

        var cart =
            await _cartRepository.GetByUserIdAsync(userId);

        if (cart == null)
            return null;

        var item =
            await _cartRepository.GetItemAsync(
                cart.Id,
                productVariantId);

        if (item == null)
            return null;

        var variant =
            await _productRepository
                .GetVariantByIdAsync(productVariantId);

        if (variant == null)
            throw new InvalidOperationException(
                "Product variant not found.");

        if (!variant.IsActive)
            throw new InvalidOperationException(
                "Product variant is not active.");

        if (quantity > variant.StockQuantity)
            throw new InvalidOperationException(
                "Insufficient stock.");

        item.Quantity = quantity;

        // Refresh price from DB.
        item.UnitPrice = variant.Price;

        await _cartRepository.UpdateItemAsync(item);
        await _unitOfWork.SaveChangesAsync();

        var updatedCart =
            await _cartRepository.GetByUserIdAsync(userId);

        if (updatedCart == null)
            return null;

        return MapToDto(updatedCart);
    }

    public async Task<bool> RemoveItemAsync(
        int userId,
        int productVariantId)
    {
        var cart =
            await _cartRepository.GetByUserIdAsync(userId);

        if (cart == null)
            return false;

        var item =
            await _cartRepository.GetItemAsync(
                cart.Id,
                productVariantId);

        if (item == null)
            return false;

        await _cartRepository.RemoveItemAsync(item);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    private static CartDto MapToDto(Cart cart)
    {
        var items = cart.Items
            .Select(item => new CartItemDto
            {
                Id = item.Id,
                ProductVariantId =
                    item.ProductVariantId,

                ProductName =
                    item.ProductVariant.Product.Name,

                Strength =
                    item.ProductVariant.Strength,

                PackSize =
                    item.ProductVariant.PackSize,

                Quantity =
                    item.Quantity,

                UnitPrice =
                    item.UnitPrice,

                TotalPrice =
                    item.UnitPrice * item.Quantity
            })
            .ToList();

        return new CartDto
        {
            Id = cart.Id,
            Items = items,

            TotalAmount =
                items.Sum(item => item.TotalPrice)
        };
    }
}