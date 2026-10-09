using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using RashePharma.Application.DTOs.Auth;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Domain.Entities;

namespace RashePharma.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordResetTokenRepository _passwordResetTokenRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;
    private readonly PasswordHasher<User> _passwordHasher;

    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IRefreshTokenService _refreshTokenService;

   public AuthService(
    IUserRepository userRepository,
    IPasswordResetTokenRepository passwordResetTokenRepository,
    IUnitOfWork unitOfWork,
    IJwtTokenService jwtTokenService,
    IEmailService emailService,
    IConfiguration configuration,
    IRefreshTokenRepository refreshTokenRepository,
    IRefreshTokenService refreshTokenService)
{
    _userRepository = userRepository;
    _passwordResetTokenRepository = passwordResetTokenRepository;
    _unitOfWork = unitOfWork;
    _jwtTokenService = jwtTokenService;
    _emailService = emailService;
    _configuration = configuration;
    _refreshTokenRepository = refreshTokenRepository;
    _refreshTokenService = refreshTokenService;

    _passwordHasher = new PasswordHasher<User>();
}

    // =========================================================
    // Register
    // =========================================================

    public async Task<AuthResultDto> RegisterAsync(
        RegisterDto dto)
    {
        var existingUser =
            await _userRepository.GetByEmailAsync(dto.Email);

        if (existingUser != null)
        {
            throw new InvalidOperationException(
                "A user with this email already exists.");
        }

        var user = new User
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Email = dto.Email,
            PhoneNumber = dto.PhoneNumber,
            Country = dto.Country,

            // New registrations are always normal Users
            // RoleId 1 = User
            // RoleId 2 = Admin
            RoleId = 1,

            IsActive = true
        };

        user.PasswordHash =
            _passwordHasher.HashPassword(
                user,
                dto.Password);

        await _userRepository.AddAsync(user);

        await _unitOfWork.SaveChangesAsync();

        // Reload user with Role navigation property
        var savedUser =
            await _userRepository.GetByEmailAsync(dto.Email);

        if (savedUser == null)
        {
            throw new InvalidOperationException(
                "User could not be loaded after registration.");
        }

        var token =
                _jwtTokenService.GenerateToken(savedUser);

        var refreshToken =
        _refreshTokenService.GenerateRefreshToken();

        var refreshTokenHash =
            _refreshTokenService.HashRefreshToken(refreshToken);

        var refreshTokenEntity = new RefreshToken
        {
            UserId = savedUser.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            TokenFamilyId = Guid.NewGuid()
        };

await _refreshTokenRepository.AddAsync(refreshTokenEntity);

        return new AuthResultDto
{
    Response = new AuthResponseDto
    {
        UserId = savedUser.Id,
        FirstName = savedUser.FirstName,
        LastName = savedUser.LastName,
        Email = savedUser.Email,
        PhoneNumber = savedUser.PhoneNumber,
        Country = savedUser.Country,
        Role = savedUser.Role?.Name ?? "User",
        Token = token
    },

    RefreshToken = refreshToken
};
    }

    // =========================================================
    // Login
    // =========================================================

    public async Task<AuthResultDto?> LoginAsync(
        LoginDto dto)
    {
        var user =
            await _userRepository.GetByEmailAsync(dto.Email);

        if (user == null || !user.IsActive)
        {
            return null;
        }

        var verificationResult =
            _passwordHasher.VerifyHashedPassword(
                user,
                user.PasswordHash,
                dto.Password);

        if (verificationResult ==
            PasswordVerificationResult.Failed)
        {
            return null;
        }

        var token =
            _jwtTokenService.GenerateToken(user);

        var refreshToken = _refreshTokenService.GenerateRefreshToken();

        var refreshTokenHash =
            _refreshTokenService.HashRefreshToken(refreshToken);

        var refreshTokenEntity = new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(30),
            TokenFamilyId = Guid.NewGuid()
        };

        await _refreshTokenRepository.AddAsync(refreshTokenEntity);

        return new AuthResultDto
{
    Response = new AuthResponseDto
    {
        UserId = user.Id,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email,
        PhoneNumber = user.PhoneNumber,
        Country = user.Country,
        Role = user.Role?.Name ?? "User",
        Token = token
    },

    RefreshToken = refreshToken
};
    }

    // =========================================================
    // Forgot Password
    // =========================================================

    public async Task ForgotPasswordAsync(string email)
    {
        var normalizedEmail = email.Trim();

        var user =
            await _userRepository.GetByEmailAsync(
                normalizedEmail);

        // Do not reveal whether the email exists.
        if (user == null || !user.IsActive)
        {
            return;
        }

        // Generate a cryptographically secure random token.
        var rawToken =
            Convert.ToBase64String(
                RandomNumberGenerator.GetBytes(32));

        // Store only the SHA-256 hash in the database.
        var tokenHash =
            Convert.ToHexString(
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(rawToken)));

        // Token is valid for 30 minutes.
        var resetToken = new PasswordResetToken
        {
            UserId = user.Id,
            TokenHash = tokenHash,
            ExpiresAt = DateTime.UtcNow.AddMinutes(30),
            CreatedAt = DateTime.UtcNow
        };

        await _passwordResetTokenRepository.AddAsync(
            resetToken);

        await _unitOfWork.SaveChangesAsync();

        // =====================================================
        // Create password reset URL
        // =====================================================

        var frontendBaseUrl =
            _configuration["Frontend:BaseUrl"];

        if (string.IsNullOrWhiteSpace(frontendBaseUrl))
        {
            throw new InvalidOperationException(
                "Frontend:BaseUrl is not configured.");
        }

        var resetUrl =
            $"{frontendBaseUrl.TrimEnd('/')}" +
            $"/reset-password?token=" +
            $"{Uri.EscapeDataString(rawToken)}";

        // =====================================================
        // Password reset email
        // =====================================================

        var firstName =
            string.IsNullOrWhiteSpace(user.FirstName)
                ? "there"
                : user.FirstName;

        var htmlBody = $"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="UTF-8">
                <title>Reset Your Password</title>
            </head>
            <body style="margin:0;padding:0;background:#f5f7f8;font-family:Arial,sans-serif;">
                <div style="max-width:600px;margin:40px auto;background:#ffffff;border-radius:12px;padding:40px;box-shadow:0 2px 10px rgba(0,0,0,0.08);">

                    <h2 style="color:#1f2937;margin-top:0;">
                        Reset Your Password
                    </h2>

                    <p style="color:#4b5563;font-size:15px;line-height:1.6;">
                        Hi {System.Net.WebUtility.HtmlEncode(firstName)},
                    </p>

                    <p style="color:#4b5563;font-size:15px;line-height:1.6;">
                        We received a request to reset your Rashe Pharma account password.
                    </p>

                    <p style="color:#4b5563;font-size:15px;line-height:1.6;">
                        Click the button below to create a new password.
                    </p>

                    <div style="margin:30px 0;">
                        <a
                            href="{System.Net.WebUtility.HtmlEncode(resetUrl)}"
                            style="display:inline-block;background:#3E8F96;color:#ffffff;text-decoration:none;padding:14px 24px;border-radius:8px;font-weight:bold;">
                            Reset Password
                        </a>
                    </div>

                    <p style="color:#6b7280;font-size:13px;line-height:1.6;">
                        This password reset link will expire in 30 minutes.
                    </p>

                    <p style="color:#6b7280;font-size:13px;line-height:1.6;">
                        If you did not request a password reset, you can safely ignore this email.
                    </p>

                    <hr style="border:none;border-top:1px solid #e5e7eb;margin:30px 0;">

                    <p style="color:#9ca3af;font-size:12px;">
                        Rashe Pharma
                    </p>

                </div>
            </body>
            </html>
            """;

        await _emailService.SendAsync(
            user.Email,
            $"{user.FirstName} {user.LastName}".Trim(),
            "Reset Your Rashe Pharma Password",
            htmlBody);
    }

    // =========================================================
    // Reset Password
    // =========================================================

    public async Task<bool> ResetPasswordAsync(
        string token,
        string newPassword)
    {
        if (string.IsNullOrWhiteSpace(token) ||
            string.IsNullOrWhiteSpace(newPassword))
        {
            return false;
        }

        var tokenHash =
            Convert.ToHexString(
                SHA256.HashData(
                    Encoding.UTF8.GetBytes(token)));

        var resetToken =
            await _passwordResetTokenRepository
                .GetByTokenHashAsync(tokenHash);

        if (resetToken == null)
        {
            return false;
        }

        // Token can only be used once.
        if (resetToken.UsedAt.HasValue)
        {
            return false;
        }

        // Token must not be expired.
        if (resetToken.ExpiresAt <= DateTime.UtcNow)
        {
            return false;
        }

        var user = resetToken.User;

        if (user == null || !user.IsActive)
        {
            return false;
        }

        user.PasswordHash =
            _passwordHasher.HashPassword(
                user,
                newPassword);

        user.UpdatedAt = DateTime.UtcNow;

        // Mark token as used.
        resetToken.UsedAt = DateTime.UtcNow;

        await _userRepository.UpdateAsync(user);

        await _passwordResetTokenRepository
            .UpdateAsync(resetToken);

        await _unitOfWork.SaveChangesAsync();

        return true;
    }


    public async Task<AuthResultDto?> RefreshAsync(
    string refreshToken)
{
    if (string.IsNullOrWhiteSpace(refreshToken))
    {
        return null;
    }

    var refreshTokenHash =
        _refreshTokenService.HashRefreshToken(
            refreshToken);

    var storedToken =
        await _refreshTokenRepository
            .FindByTokenHashAsync(refreshTokenHash);

    if (storedToken == null)
    {
        return null;
    }

    // Refresh token has expired.
    if (storedToken.ExpiresAt <= DateTime.UtcNow)
    {
        return null;
    }

    // Token was already revoked/rotated.
    // This may indicate refresh-token reuse.
    if (storedToken.RevokedAt.HasValue)
    {
        await _refreshTokenRepository
            .RevokeFamilyAsync(
                storedToken.TokenFamilyId);

        return null;
    }

    var user =
        await _userRepository.GetByIdAsync(
            storedToken.UserId);

    if (user == null || !user.IsActive)
    {
        return null;
    }

    var accessToken =
        _jwtTokenService.GenerateToken(user);

    var newRefreshToken =
        _refreshTokenService.GenerateRefreshToken();

    var newRefreshTokenHash =
        _refreshTokenService.HashRefreshToken(
            newRefreshToken);

    var newRefreshTokenEntity = new RefreshToken
    {
        UserId = user.Id,
        TokenHash = newRefreshTokenHash,
        ExpiresAt = DateTime.UtcNow.AddDays(30),
        TokenFamilyId = storedToken.TokenFamilyId
    };

    // Atomically:
    // 1. Revoke current refresh token
    // 2. Store the replacement token
    // 3. Commit both operations in one transaction
    var rotated =
        await _refreshTokenRepository.RotateAsync(
            storedToken,
            newRefreshTokenEntity);

    // Another request already rotated this token.
    if (!rotated)
    {
        await _refreshTokenRepository
            .RevokeFamilyAsync(
                storedToken.TokenFamilyId);

        return null;
    }

    return new AuthResultDto
    {
        Response = new AuthResponseDto
        {
            UserId = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Country = user.Country,
            Role = user.Role?.Name ?? "User",
            Token = accessToken
        },

        RefreshToken = newRefreshToken
    };
}

    
    public async Task LogoutAsync(string refreshToken)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return;
        }

        var refreshTokenHash =
            _refreshTokenService.HashRefreshToken(refreshToken);

        var storedToken =
            await _refreshTokenRepository
                .FindByTokenHashAsync(refreshTokenHash);

        if (storedToken == null)
        {
            return;
        }

        await _refreshTokenRepository
            .RevokeFamilyAsync(storedToken.TokenFamilyId);
    }

        // =========================================================
        // Current User Profile
        // =========================================================

    public async Task<UserProfileDto?> GetProfileAsync(
        int userId)
    {
        var user =
            await _userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            return null;
        }

        return new UserProfileDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Country = user.Country,
            Role = user.Role?.Name ?? "User",
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt
        };
    }
}