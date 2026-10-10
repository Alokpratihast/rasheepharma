
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using RashePharma.Application.DTOs.Auth;
using RashePharma.Application.Interfaces.Services;
using System.Security.Claims;
using RashePharma.Infrastructure.Services;
using System.IdentityModel.Tokens.Jwt;
namespace RashePharma.Api.Controllers;
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly AccessTokenRevocationService _accessTokenRevocationService;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public AuthController(
        IAuthService authService,
        AccessTokenRevocationService accessTokenRevocationService,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        _authService = authService;
        _accessTokenRevocationService = accessTokenRevocationService;
        _configuration = configuration;
        _environment = environment;
    }

    // =========================================================
    // Register
    // =========================================================

    [EnableRateLimiting("auth-register")]
[HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(
        RegisterDto dto)
    {
        try
        {
            var result =
                await _authService.RegisterAsync(dto);

            SetRefreshTokenCookie(result.RefreshToken);

            return Ok(result.Response);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    // =========================================================
    // Login
    // =========================================================

    [EnableRateLimiting("auth-login")]
[HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(
        LoginDto dto)
    {
        var result =
            await _authService.LoginAsync(dto);

        if (result == null)
        {
            return Unauthorized(
                "Invalid email or password.");
        }

        SetRefreshTokenCookie(result.RefreshToken);

        return Ok(result.Response);
    }

    // =========================================================
    // Refresh Token Cookie
    // =========================================================

    private void SetRefreshTokenCookie(string refreshToken)
    {
        Response.Cookies.Append(
            "refreshToken",
            refreshToken,
            CreateRefreshCookieOptions());
    }

    private CookieOptions CreateRefreshCookieOptions()
    {
        var isDevelopment = _environment.IsDevelopment();

        return new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDevelopment,
            // Production may use a separate frontend/API origin; Origin validation below protects cookie endpoints from CSRF.
            SameSite = isDevelopment
                ? SameSiteMode.Lax
                : SameSiteMode.None,
            Expires = DateTimeOffset.UtcNow.AddDays(30),
            MaxAge = TimeSpan.FromDays(30),
            Path = "/api/Auth"
        };
    }

    private void DeleteRefreshTokenCookie()
    {
        var options = CreateRefreshCookieOptions();
        options.Expires = DateTimeOffset.UnixEpoch;
        options.MaxAge = TimeSpan.Zero;

        // Deletion uses the same path and security attributes as the original cookie.
        Response.Cookies.Append("refreshToken", string.Empty, options);
    }

    private bool IsTrustedCookieOrigin()
    {
        var requestOrigin = NormalizeOrigin(
            Request.Headers["Origin"].ToString());

        if (requestOrigin == null)
        {
            return false;
        }

        var allowedOrigins = _configuration
            .GetSection("Cors:AllowedOrigins")
            .Get<string[]>();

        if ((allowedOrigins == null || allowedOrigins.Length == 0) &&
            _environment.IsDevelopment())
        {
            allowedOrigins =
            [
                "http://localhost:3000",
                "https://rasheepharma.vercel.app"
            ];
        }

        // Cookie-authenticated endpoints must match the same explicit origin allowlist used by CORS.
        return allowedOrigins?.Any(origin =>
            string.Equals(
                NormalizeOrigin(origin),
                requestOrigin,
                StringComparison.OrdinalIgnoreCase)) == true;
    }

    private static string? NormalizeOrigin(string origin)
    {
        if (!Uri.TryCreate(origin, UriKind.Absolute, out var parsed) ||
            parsed.AbsolutePath != "/" ||
            !string.IsNullOrEmpty(parsed.Query) ||
            !string.IsNullOrEmpty(parsed.Fragment))
        {
            return null;
        }

        return parsed.GetLeftPart(UriPartial.Authority);
    }
    // =========================================================
    // Forgot Password
    // =========================================================

    [EnableRateLimiting("auth-recovery")]
[HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        ForgotPasswordDto dto)
    {
        await _authService.ForgotPasswordAsync(
            dto.Email);

        // Always return the same response so that
        // the API does not reveal whether an email
        // address exists in the database.
        return Ok(new
        {
            message =
                "If an account exists with this email, " +
                "you will receive a password reset link."
        });
    }

    // =========================================================
    // Reset Password
    // =========================================================

    [EnableRateLimiting("auth-recovery")]
[HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
        ResetPasswordDto dto)
    {
        var success =
            await _authService.ResetPasswordAsync(
                dto.Token,
                dto.NewPassword);

        if (!success)
        {
            return BadRequest(new
            {
                message =
                    "Invalid or expired password reset link."
            });
        }

        return Ok(new
        {
            message =
                "Password reset successfully."
        });
    }

    // =========================================================
    // Get Current User Profile
    // =========================================================

    [Authorize]
    [HttpGet("profile")]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        // Get the logged-in user's ID from the JWT token.
        var userIdClaim =
            User.FindFirst(
                ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
        {
            return Unauthorized();
        }

        if (!int.TryParse(
                userIdClaim.Value,
                out var userId))
        {
            return Unauthorized();
        }

        var profile =
            await _authService.GetProfileAsync(userId);

        if (profile == null)
        {
            return NotFound();
        }

        return Ok(profile);
    }


    [EnableRateLimiting("auth-refresh")]
[HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> Refresh()
    {
        // Refresh relies on an ambient HttpOnly cookie, so require a trusted browser origin.
        if (!IsTrustedCookieOrigin())
        {
            return StatusCode(
                StatusCodes.Status403Forbidden,
                new { message = "Request origin is not allowed." });
        }

        var refreshToken =
            Request.Cookies["refreshToken"];

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            return Unauthorized();
        }

        var result =
            await _authService.RefreshAsync(
                refreshToken);

        if (result == null)
        {
            DeleteRefreshTokenCookie();

            return Unauthorized();
        }

        SetRefreshTokenCookie(
            result.RefreshToken);

        return Ok(result.Response);
}


    

[EnableRateLimiting("auth-refresh")]
[HttpPost("logout")]
public async Task<IActionResult> Logout()
{
    var refreshToken = Request.Cookies["refreshToken"];

    // Cookie-based logout requires an allowlisted origin to prevent cross-site session termination.
    if (!string.IsNullOrWhiteSpace(refreshToken) &&
        !IsTrustedCookieOrigin())
    {
        return StatusCode(
            StatusCodes.Status403Forbidden,
            new { message = "Request origin is not allowed." });
    }

    // Revoke the refresh-token family.

    if (!string.IsNullOrWhiteSpace(refreshToken))
    {
        await _authService.LogoutAsync(refreshToken);
    }

    // Revoke the current access JWT, if a valid
    // authenticated JWT is available on this request.
    var jti = User.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;

    var expClaim = User.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;

    if (!string.IsNullOrWhiteSpace(jti) &&
        long.TryParse(expClaim, out var exp))
    {
        var expiresAtUtc =
            DateTimeOffset.FromUnixTimeSeconds(exp).UtcDateTime;

        await _accessTokenRevocationService.RevokeAsync(
            jti,
            expiresAtUtc,
            HttpContext.RequestAborted);
    }

    DeleteRefreshTokenCookie();

    return Ok(new { message = "Logged out successfully." });
}


}