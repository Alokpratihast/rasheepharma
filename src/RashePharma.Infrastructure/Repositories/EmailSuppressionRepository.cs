
using Microsoft.EntityFrameworkCore;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Domain.Entities.Email;
using RashePharma.Infrastructure.Data;

namespace RashePharma.Infrastructure.Repositories;

public sealed class EmailSuppressionRepository
    : IEmailSuppressionRepository
{
    private readonly ApplicationDbContext _context;

    public EmailSuppressionRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<EmailSuppression?> GetByIdAsync(Guid id)
    {
        return await _context.Set<EmailSuppression>()
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<EmailSuppression?> GetByEmailAsync(string email)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return await _context.Set<EmailSuppression>()
            .FirstOrDefaultAsync(
                x => x.EmailNormalized .ToLower() == normalizedEmail);
    }

    public async Task AddAsync(EmailSuppression suppression)
    {
        await _context.Set<EmailSuppression>()
            .AddAsync(suppression);
    }

    public Task UpdateAsync(EmailSuppression suppression)
    {
        _context.Set<EmailSuppression>().Update(suppression);
        return Task.CompletedTask;
    }
}
