namespace RashePharma.Application.DTOs.Auth;

public class AuthResponseDto
{
    public int UserId { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? Country { get; set; }

    public string Role { get; set; } = string.Empty;

    public string Token { get; set; } = string.Empty;
}