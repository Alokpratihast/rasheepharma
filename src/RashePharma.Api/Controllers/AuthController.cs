using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RashePharma.Application.DTOs.Auth;
using RashePharma.Application.Interfaces.Services;
using System.Security.Claims;

namespace RashePharma.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
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
            var response = await _authService.RegisterAsync(dto);

            return Ok(response);
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
        var response = await _authService.LoginAsync(dto);

        if (response == null)
            return Unauthorized("Invalid email or password.");

        return Ok(response);
    }


    // =========================================================
    // Get Current User Profile
    // =========================================================

    [Authorize]
    [HttpGet("profile")]
    public async Task<ActionResult<UserProfileDto>> GetProfile()
    {
        // Get the logged-in user's ID from the JWT token.
        var userIdClaim = User.FindFirst(
            ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
            return Unauthorized();

        if (!int.TryParse(
                userIdClaim.Value,
                out var userId))
        {
            return Unauthorized();
        }

        var profile = await _authService.GetProfileAsync(userId);

        if (profile == null)
            return NotFound();

        return Ok(profile);
    }
}