
using Microsoft.EntityFrameworkCore;
using RashePharma.Infrastructure.Data;

namespace RashePharma.Infrastructure.Services;

public class AccessTokenRevocationService
{
    private readonly ApplicationDbContext _db;

    public AccessTokenRevocationService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<bool> IsRevokedAsync(
        string jti,
        CancellationToken cancellationToken = default)
    {
        return await _db.RevokedAccessTokens
            .AnyAsync(
                token => token.Jti == jti,
                cancellationToken);
    }

    public async Task RevokeAsync(
        string jti,
        DateTime expiresAtUtc,
        CancellationToken cancellationToken = default)
    {
        var alreadyRevoked = await _db.RevokedAccessTokens
            .AnyAsync(
                token => token.Jti == jti,
                cancellationToken);

        if (alreadyRevoked)
            return;

        _db.RevokedAccessTokens.Add(
            new RashePharma.Domain.Entities.RevokedAccessToken
            {
                Jti = jti,
                ExpiresAtUtc = expiresAtUtc,
                RevokedAtUtc = DateTime.UtcNow
            });

        await _db.SaveChangesAsync(cancellationToken);
    }
}