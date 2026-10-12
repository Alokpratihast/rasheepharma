namespace RashePharma.Domain.Entities;

public class RevokedAccessToken
{
    public string Jti { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime RevokedAtUtc { get; set; } = DateTime.UtcNow;
}
