namespace RashePharma.Application.DTOs.Auth;

public class RegisterDto
{
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string? PhoneNumber { get; set; }

    public string? Country { get; set; }

    public string Password { get; set; } = string.Empty;
}