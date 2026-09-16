using Microsoft.EntityFrameworkCore;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;

namespace RashePharma.Infrastructure.Repositories;

public class WebsiteContentRepository : IWebsiteContentRepository
{
    private readonly ApplicationDbContext _context;

    public WebsiteContentRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<WebsiteContent>> GetAllAsync()
    {
        return await _context.WebsiteContents
            .OrderBy(c => c.Section)
            .ThenBy(c => c.DisplayOrder)
            .ThenBy(c => c.Id)
            .ToListAsync();
    }

    public async Task<List<WebsiteContent>> GetBySectionAsync(
        string section)
    {
        return await _context.WebsiteContents
            .Where(c => c.Section == section)
            .OrderBy(c => c.DisplayOrder)
            .ThenBy(c => c.Id)
            .ToListAsync();
    }

    public async Task<WebsiteContent?> GetByIdAsync(int id)
    {
        return await _context.WebsiteContents
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<WebsiteContent?> GetByKeyAsync(
        string section,
        string key)
    {
        return await _context.WebsiteContents
            .FirstOrDefaultAsync(c =>
                c.Section == section &&
                c.Key == key);
    }

    public async Task AddAsync(WebsiteContent content)
    {
        await _context.WebsiteContents.AddAsync(content);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(WebsiteContent content)
    {
        _context.WebsiteContents.Update(content);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(WebsiteContent content)
    {
        _context.WebsiteContents.Remove(content);
        await _context.SaveChangesAsync();
    }
}