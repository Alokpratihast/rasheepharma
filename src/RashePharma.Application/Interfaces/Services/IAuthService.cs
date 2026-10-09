using RashePharma.Application.DTOs.Auth;

namespace RashePharma.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResultDto> RegisterAsync(RegisterDto dto);
    Task<AuthResultDto?> LoginAsync(LoginDto dto);
    Task<UserProfileDto?> GetProfileAsync(int userId);

    Task<AuthResultDto?> RefreshAsync(string refreshToken);

    
    Task LogoutAsync(string refreshToken);


    // Password reset
    Task ForgotPasswordAsync(string email);

    Task<bool> ResetPasswordAsync(
        string token,
        string newPassword);
}