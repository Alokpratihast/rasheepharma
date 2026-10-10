using RashePharma.Domain.Entities;

namespace RashePharma.Application.Interfaces;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> FindByTokenHashAsync(
        string tokenHash);

    Task<bool> RotateAsync(
        RefreshToken currentToken,
        RefreshToken newToken);

    Task AddAsync(
        RefreshToken refreshToken);

    Task RevokeAsync(
        RefreshToken refreshToken);

    Task RevokeFamilyAsync(
        Guid tokenFamilyId);

    Task RevokeAllForUserAsync(int userId);
}