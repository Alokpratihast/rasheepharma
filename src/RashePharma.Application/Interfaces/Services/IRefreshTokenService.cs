namespace RashePharma.Application.Interfaces.Services;

public interface IRefreshTokenService
{
    string GenerateRefreshToken();

    string HashRefreshToken(string refreshToken);
}