
using Microsoft.EntityFrameworkCore;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Domain.Entities.Email;
using RashePharma.Infrastructure.Data;

namespace RashePharma.Infrastructure.Repositories;

public sealed class EmailQuotaBucketRepository
    : IEmailQuotaBucketRepository
{
    private readonly ApplicationDbContext _context;

    public EmailQuotaBucketRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmailQuotaBucket?> GetByIdAsync(Guid id)
    {
        return await _context.Set<EmailQuotaBucket>()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task AddAsync(EmailQuotaBucket quotaBucket)
    {
        await _context.Set<EmailQuotaBucket>()
            .AddAsync(quotaBucket);
    }

    public Task UpdateAsync(EmailQuotaBucket quotaBucket)
    {
        _context.Set<EmailQuotaBucket>().Update(quotaBucket);
        return Task.CompletedTask;
    }
}
