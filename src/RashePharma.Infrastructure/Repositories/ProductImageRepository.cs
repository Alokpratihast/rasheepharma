using Microsoft.EntityFrameworkCore;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;

namespace RashePharma.Infrastructure.Repositories;

public class ProductImageRepository : IProductImageRepository
{
    private readonly ApplicationDbContext _context;

    public ProductImageRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ProductImage>> GetByProductIdAsync(int productId)
    {
        return await _context.ProductImages
            .Where(i => i.ProductId == productId)
            .OrderBy(i => i.DisplayOrder)
            .ToListAsync();
    }

    public async Task<ProductImage?> GetByIdAsync(int id)
    {
        return await _context.ProductImages
            .FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task AddAsync(ProductImage image)
    {
        await _context.ProductImages.AddAsync(image);
    }

    public Task UpdateAsync(ProductImage image)
    {
        _context.ProductImages.Update(image);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ProductImage image)
    {
        _context.ProductImages.Remove(image);
        return Task.CompletedTask;
    }
}