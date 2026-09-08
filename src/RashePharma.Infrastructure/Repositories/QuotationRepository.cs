using Microsoft.EntityFrameworkCore;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;

namespace RashePharma.Infrastructure.Repositories;

public class QuotationRepository : IQuotationRepository
{
    private readonly ApplicationDbContext _context;

    public QuotationRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Quotation>> GetAllAsync()
    {
        return await _context.Quotations
            .Include(q => q.Enquiry)
            .Include(q => q.Items)
                .ThenInclude(i => i.ProductVariant)
                    .ThenInclude(v => v.Product)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Quotation>> GetByUserIdAsync(
        int userId)
    {
        return await _context.Quotations
            .Include(q => q.Enquiry)
            .Include(q => q.Items)
                .ThenInclude(i => i.ProductVariant)
                    .ThenInclude(v => v.Product)
            .Where(q => q.UserId == userId)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Quotation>> GetByEnquiryIdAsync(
        int enquiryId)
    {
        return await _context.Quotations
            .Include(q => q.Enquiry)
            .Include(q => q.Items)
                .ThenInclude(i => i.ProductVariant)
                    .ThenInclude(v => v.Product)
            .Where(q => q.EnquiryId == enquiryId)
            .OrderByDescending(q => q.CreatedAt)
            .ToListAsync();
    }

    public async Task<Quotation?> GetByIdAsync(int id)
    {
        return await _context.Quotations
            .Include(q => q.Enquiry)
            .Include(q => q.Items)
                .ThenInclude(i => i.ProductVariant)
                    .ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(q => q.Id == id);
    }

    public async Task<Quotation?> GetByQuoteNumberAsync(
        string quoteNumber)
    {
        return await _context.Quotations
            .Include(q => q.Enquiry)
            .Include(q => q.Items)
                .ThenInclude(i => i.ProductVariant)
                    .ThenInclude(v => v.Product)
            .FirstOrDefaultAsync(
                q => q.QuoteNumber == quoteNumber);
    }

    public async Task AddAsync(Quotation quotation)
    {
        await _context.Quotations.AddAsync(quotation);
    }

    public async Task UpdateAsync(Quotation quotation)
    {
        _context.Quotations.Update(quotation);

        await Task.CompletedTask;
    }
}