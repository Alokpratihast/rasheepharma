using Microsoft.EntityFrameworkCore;
using RashePharma.Application.Interfaces;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;

namespace RashePharma.Infrastructure.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly ApplicationDbContext _context;

    public RefreshTokenRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RefreshToken?> FindByTokenHashAsync(
        string tokenHash)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.TokenHash == tokenHash);
    }

   

    public async Task AddAsync(
        RefreshToken refreshToken)
    {
        await _context.RefreshTokens.AddAsync(
            refreshToken);

        await _context.SaveChangesAsync();
    }

    public async Task RevokeFamilyAsync(
        Guid tokenFamilyId)
    {
        await _context.RefreshTokens
            .Where(t =>
                t.TokenFamilyId == tokenFamilyId &&
                t.RevokedAt == null)
            .ExecuteUpdateAsync(setters =>
                setters.SetProperty(
                    t => t.RevokedAt,
                    DateTime.UtcNow));
    }


    public async Task RevokeAsync(
        RefreshToken refreshToken)
    {
        refreshToken.RevokedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    public async Task<bool> RotateAsync(
    RefreshToken currentToken,
    RefreshToken newToken)
{
    await using var transaction =
        await _context.Database.BeginTransactionAsync();

    try
    {
        var affectedRows =
            await _context.RefreshTokens
                .Where(t =>
                    t.Id == currentToken.Id &&
                    t.RevokedAt == null &&
                    t.RowVersion == currentToken.RowVersion)
                .ExecuteUpdateAsync(setters =>
                    setters
                        .SetProperty(
                            t => t.RevokedAt,
                            DateTime.UtcNow)
                        .SetProperty(
                            t => t.ReplacedByTokenHash,
                            newToken.TokenHash));

        // Another request already rotated this token.
        if (affectedRows != 1)
        {
            await transaction.RollbackAsync();
            return false;
        }

        await _context.RefreshTokens.AddAsync(newToken);

        await _context.SaveChangesAsync();

        await transaction.CommitAsync();

        return true;
    }
    catch
    {
        await transaction.RollbackAsync();
        throw;
    }
}
}