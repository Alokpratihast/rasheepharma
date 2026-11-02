using System.Security.Cryptography;
using System.Text;

namespace RashePharma.Infrastructure.Services;

/// <summary>Signs short-lived, recipient-specific links used to submit one campaign reply.</summary>
public static class LeadOutreachResponseToken
{
    public static string Create(int recipientId, int leadId, DateTime expiresAtUtc, string secret)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(secret);
        var payload = $"{recipientId}.{leadId}.{new DateTimeOffset(expiresAtUtc).ToUnixTimeSeconds()}";
        var signature = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(payload));
        return $"{payload}.{Convert.ToHexString(signature)}";
    }

    public static bool TryRead(string? token, string secret, DateTime nowUtc, out int recipientId, out int leadId)
    {
        recipientId = 0;
        leadId = 0;
        if (string.IsNullOrWhiteSpace(token) || string.IsNullOrWhiteSpace(secret)) return false;
        var parts = token.Split('.');
        if (parts.Length != 4 || !int.TryParse(parts[0], out recipientId) || recipientId <= 0 ||
            !int.TryParse(parts[1], out leadId) || leadId <= 0 ||
            !long.TryParse(parts[2], out var expiryUnix)) return false;
        DateTime expiry;
        try { expiry = DateTimeOffset.FromUnixTimeSeconds(expiryUnix).UtcDateTime; }
        catch (ArgumentOutOfRangeException) { return false; }
        if (expiry <= nowUtc) return false;
        var payload = $"{parts[0]}.{parts[1]}.{parts[2]}";
        var expected = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(payload));
        byte[] supplied;
        try { supplied = Convert.FromHexString(parts[3]); }
        catch (FormatException) { return false; }
        return supplied.Length == expected.Length && CryptographicOperations.FixedTimeEquals(supplied, expected);
    }
}
