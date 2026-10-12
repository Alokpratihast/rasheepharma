using Microsoft.EntityFrameworkCore;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;

namespace RashePharma.Infrastructure.Repositories;

public class PasswordResetTokenRepository : IPasswordResetTokenRepository
{
    private readonly ApplicationDbContext _context;

    public PasswordResetTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(PasswordResetToken token)
    {
        await _context.PasswordResetTokens.AddAsync(token);
    }

    public async Task<PasswordResetToken?> GetByTokenHashAsync(
        string tokenHash)
    {
        return await _context.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
    }

    public async Task UpdateAsync(PasswordResetToken token)
    {
        _context.PasswordResetTokens.Update(token);

        await Task.CompletedTask;
    }
}