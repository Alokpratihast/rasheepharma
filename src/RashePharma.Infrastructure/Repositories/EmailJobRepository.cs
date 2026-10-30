
using Microsoft.EntityFrameworkCore;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Domain.Entities.Email;
using RashePharma.Infrastructure.Data;

namespace RashePharma.Infrastructure.Repositories;

public sealed class EmailJobRepository : IEmailJobRepository
{
    private readonly ApplicationDbContext _context;

    public EmailJobRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmailJob?> GetByIdAsync(Guid id)
    {
        return await _context.Set<EmailJob>()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<EmailJob?> GetByIdempotencyKeyAsync(
        string idempotencyKey)
    {
        return await _context.Set<EmailJob>()
            .FirstOrDefaultAsync(
                x => x.IdempotencyKey == idempotencyKey);
    }

    public async Task AddAsync(EmailJob emailJob)
    {
        await _context.Set<EmailJob>().AddAsync(emailJob);
    }

    public Task UpdateAsync(EmailJob emailJob)
    {
        _context.Set<EmailJob>().Update(emailJob);
        return Task.CompletedTask;
    }
}
