using System.Security.Cryptography;
using System.Text;

using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Infrastructure.Services;

public class RefreshTokenService : IRefreshTokenService
{
    public string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);

        return Convert.ToBase64String(randomBytes);
    }

    public string HashRefreshToken(string refreshToken)
    {
        var bytes = Encoding.UTF8.GetBytes(refreshToken);

        var hash = SHA256.HashData(bytes);

        return Convert.ToHexString(hash);
    }
}