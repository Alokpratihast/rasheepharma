using Microsoft.EntityFrameworkCore;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;

namespace RashePharma.Infrastructure.Repositories;

public class EnquiryRepository : IEnquiryRepository
{
    private readonly ApplicationDbContext _context;

    public EnquiryRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Enquiry>> GetAllAsync()
    {
        return await _context.Enquiries
            .Include(e => e.Items)
                .ThenInclude(i => i.ProductVariant)
                    .ThenInclude(v => v.Product)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Enquiry>> GetByUserIdAsync(int userId)
    {
        return await _context.Enquiries
            .Include(e => e.Items)
                .ThenInclude(i => i.ProductVariant)
                    .ThenInclude(v => v.Product)
            .Where(e => e.UserId == userId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();
    }

    public async Task<Enquiry?> GetByIdAsync(int id)
    {
        return await _context.Enquiries
            .Include(e => e.Items)
                .ThenInclude(i => i.ProductVariant)
                    .ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    public async Task<Enquiry?> GetByEnquiryNumberAsync(string enquiryNumber)
    {
        return await _context.Enquiries
            .Include(e => e.Items)
                .ThenInclude(i => i.ProductVariant)
                    .ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(e => e.EnquiryNumber == enquiryNumber);
    }

    public async Task AddAsync(Enquiry enquiry)
    {
        await _context.Enquiries.AddAsync(enquiry);
    }

    public async Task UpdateAsync(Enquiry enquiry)
    {
        _context.Enquiries.Update(enquiry);

        await Task.CompletedTask;
    }
}