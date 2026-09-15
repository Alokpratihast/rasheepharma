using Microsoft.EntityFrameworkCore;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;

namespace RashePharma.Infrastructure.Repositories;

public class ProductVariantRepository : IProductVariantRepository
{
    private readonly ApplicationDbContext _context;

    public ProductVariantRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductVariant>> GetByProductIdAsync(int productId)
    {
        return await _context.ProductVariants
            .Where(v => v.ProductId == productId)
            .OrderBy(v => v.Id)
            .ToListAsync();
    }

    public async Task<ProductVariant?> GetByIdAsync(int id)
    {
        return await _context.ProductVariants
            .Include(v => v.Product)
            .FirstOrDefaultAsync(v => v.Id == id);
    }

    public async Task AddAsync(ProductVariant variant)
    {
        await _context.ProductVariants.AddAsync(variant);
    }

    public Task UpdateAsync(ProductVariant variant)
    {
        _context.ProductVariants.Update(variant);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ProductVariant variant)
    {
        _context.ProductVariants.Remove(variant);
        return Task.CompletedTask;
    }
}