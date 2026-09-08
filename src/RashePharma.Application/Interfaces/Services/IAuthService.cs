using RashePharma.Application.DTOs.Auth;

namespace RashePharma.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto?> LoginAsync(LoginDto dto);
    Task<UserProfileDto?> GetProfileAsync(int userId);
}