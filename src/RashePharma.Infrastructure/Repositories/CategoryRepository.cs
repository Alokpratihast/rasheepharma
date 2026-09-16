using Microsoft.EntityFrameworkCore;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;

namespace RashePharma.Infrastructure.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly ApplicationDbContext _context;

    public CategoryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        return await _context.Categories
            .Include(c => c.ParentCategory)
            .ToListAsync();
    }

    public async Task<List<Category>> GetNavigationAsync()
{
    return await _context.Categories
        .Include(c => c.Children)
        .Include(c => c.Products)
        .ToListAsync();
}

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Category?> GetBySlugAsync(string slug)
    {
        return await _context.Categories
            .Include(c => c.ParentCategory)
            .FirstOrDefaultAsync(c => c.Slug == slug);
    }

    public async Task AddAsync(Category category)
    {
        await _context.Categories.AddAsync(category);
    }

    public async Task UpdateAsync(Category category)
    {
        _context.Categories.Update(category);

        await Task.CompletedTask;
    }

    public async Task DeleteAsync(Category category)
    {
        _context.Categories.Remove(category);

        await Task.CompletedTask;
    }

    public async Task<bool> ExistsBySlugAsync(string slug)
    {
        return await _context.Categories
            .AnyAsync(c => c.Slug == slug);
    }

    public async Task<int> GetTotalCountAsync()
{
    return await _context.Categories.CountAsync();
}

public async Task<bool> HasChildrenAsync(int categoryId)
{
    return await _context.Categories
        .AnyAsync(c => c.ParentCategoryId == categoryId);
}

public async Task<bool> HasProductsAsync(int categoryId)
{
    return await _context.Products
        .AnyAsync(p => p.CategoryId == categoryId);
}
}