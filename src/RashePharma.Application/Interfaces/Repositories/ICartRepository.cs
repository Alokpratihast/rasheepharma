using RashePharma.Domain.Entities;

namespace RashePharma.Application.Interfaces.Repositories;

public interface ICartRepository
{
    Task<Cart?> GetByUserIdAsync(int userId);

    Task<Cart?> GetByIdAsync(int id);

    Task AddAsync(Cart cart);

    Task UpdateAsync(Cart cart);

    Task<CartItem?> GetItemAsync(int cartId, int productVariantId);

    Task AddItemAsync(CartItem item);

    Task UpdateItemAsync(CartItem item);

    Task RemoveItemAsync(CartItem item);

    Task ClearItemsAsync(int cartId);
}