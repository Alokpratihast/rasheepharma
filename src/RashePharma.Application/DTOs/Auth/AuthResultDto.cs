namespace RashePharma.Application.DTOs.Auth;

public class AuthResultDto
{
    public AuthResponseDto Response { get; set; } = null!;

    public string RefreshToken { get; set; } = string.Empty;
}