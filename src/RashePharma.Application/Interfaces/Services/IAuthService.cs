using RashePharma.Application.DTOs.Auth;

namespace RashePharma.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
    Task<AuthResponseDto?> LoginAsync(LoginDto dto);
    Task<UserProfileDto?> GetProfileAsync(int userId);

    // Password reset
    Task ForgotPasswordAsync(string email);

    Task<bool> ResetPasswordAsync(
        string token,
        string newPassword);
}