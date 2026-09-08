using Microsoft.EntityFrameworkCore;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;

namespace RashePharma.Infrastructure.Repositories;

public class CartRepository : ICartRepository
{
    private readonly ApplicationDbContext _context;

    public CartRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Cart?> GetByUserIdAsync(int userId)
    {
        return await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.ProductVariant)
                    .ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);
    }

    public async Task<Cart?> GetByIdAsync(int id)
    {
        return await _context.Carts
            .Include(c => c.Items)
                .ThenInclude(i => i.ProductVariant)
                    .ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddAsync(Cart cart)
    {
        await _context.Carts.AddAsync(cart);
    }

    public async Task UpdateAsync(Cart cart)
    {
        _context.Carts.Update(cart);

        await Task.CompletedTask;
    }

    public async Task<CartItem?> GetItemAsync(
        int cartId,
        int productVariantId)
    {
        return await _context.CartItems
            .FirstOrDefaultAsync(item =>
                item.CartId == cartId &&
                item.ProductVariantId == productVariantId);
    }

    public async Task AddItemAsync(CartItem item)
    {
        await _context.CartItems.AddAsync(item);
    }

    public async Task UpdateItemAsync(CartItem item)
    {
        _context.CartItems.Update(item);

        await Task.CompletedTask;
    }

    public async Task RemoveItemAsync(CartItem item)
    {
        _context.CartItems.Remove(item);

        await Task.CompletedTask;
    }

    

    public async Task ClearItemsAsync(int cartId)
{
    var items = await _context.CartItems
        .Where(item => item.CartId == cartId)
        .ToListAsync();

    _context.CartItems.RemoveRange(items);
}
}