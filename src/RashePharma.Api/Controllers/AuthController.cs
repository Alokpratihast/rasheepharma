
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RashePharma.Application.DTOs.Auth;
using RashePharma.Application.Interfaces.Services;
using System.Security.Claims;
using RashePharma.Infrastructure.Services;
using System.IdentityModel.Tokens.Jwt;
namespace RashePharma.Api.Controllers;
using Microsoft.AspNetCore.Hosting;


[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly AccessTokenRevocationService _accessTokenRevocationService;
   public AuthController(
    IAuthService authService,
    AccessTokenRevocationService accessTokenRevocationService)
{
    _authService = authService;
    _accessTokenRevocationService = accessTokenRevocationService;
}

    // =========================================================
    // Register
    // =========================================================

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
    var isDevelopment =
        HttpContext.RequestServices
            .GetRequiredService<IWebHostEnvironment>()
            .IsDevelopment();

    Response.Cookies.Append(
        "refreshToken",
        refreshToken,
        new CookieOptions
        {
            HttpOnly = true,

            // Local HTTP development needs Secure=false.
            // Production must use Secure=true.
            Secure = !isDevelopment,

            SameSite = SameSiteMode.Lax,

            Expires =
                DateTimeOffset.UtcNow.AddDays(30),

            Path = "/api/Auth"
        });
}

    // =========================================================
    // Forgot Password
    // =========================================================

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


    [HttpPost("refresh")]
    public async Task<ActionResult<AuthResponseDto>> Refresh()
    {
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
            Response.Cookies.Delete(
                "refreshToken",
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Lax,
                    Path = "/api/Auth"
                });

            return Unauthorized();
        }

        SetRefreshTokenCookie(
            result.RefreshToken);

        return Ok(result.Response);
}


    

[HttpPost("logout")]
public async Task<IActionResult> Logout()
{
    // Revoke the refresh-token family.
    var refreshToken = Request.Cookies["refreshToken"];

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

    // Delete the refresh-token cookie.
    var isDevelopment = HttpContext.RequestServices
        .GetRequiredService<IWebHostEnvironment>()
        .IsDevelopment();

    Response.Cookies.Delete(
        "refreshToken",
        new CookieOptions
        {
            HttpOnly = true,
            Secure = !isDevelopment,
            SameSite = SameSiteMode.Lax,
            Path = "/api/Auth"
        });

    return Ok(new { message = "Logged out successfully." });
}


}