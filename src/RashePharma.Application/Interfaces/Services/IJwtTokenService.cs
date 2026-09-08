using RashePharma.Domain.Entities;

namespace RashePharma.Application.Interfaces.Services;

public interface IJwtTokenService
{
    string GenerateToken(User user);
}