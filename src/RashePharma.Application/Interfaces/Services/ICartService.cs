using RashePharma.Application.DTOs.Cart;

namespace RashePharma.Application.Interfaces.Services;

public interface ICartService
{
    Task<CartDto> GetCartAsync(int userId);
    Task<CartDto> AddToCartAsync(int userId, AddToCartDto dto);
    Task<CartDto?> UpdateItemAsync(int userId, int productVariantId, int quantity);
    Task<bool> RemoveItemAsync(int userId, int productVariantId);
}