using System.Security.Cryptography;
using System.Text;

namespace RashePharma.Infrastructure.Services;

public static class LeadUnsubscribeToken
{
    public static string Create(int recipientId, int leadId, string secret)
    {
        var payload = $"{recipientId}.{leadId}";
        var signature = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(payload));
        return $"{payload}.{Convert.ToHexString(signature)}";
    }

    public static bool TryRead(string token, string secret, out int recipientId, out int leadId)
    {
        recipientId = 0;
        leadId = 0;
        var parts = token.Split('.');
        if (parts.Length != 3 || !int.TryParse(parts[0], out recipientId) ||
            !int.TryParse(parts[1], out leadId) || recipientId <= 0 || leadId <= 0)
            return false;
        var payload = $"{parts[0]}.{parts[1]}";
        var expected = HMACSHA256.HashData(Encoding.UTF8.GetBytes(secret), Encoding.UTF8.GetBytes(payload));
        byte[] supplied;
        try { supplied = Convert.FromHexString(parts[2]); }
        catch (FormatException) { return false; }
        return supplied.Length == expected.Length && CryptographicOperations.FixedTimeEquals(supplied, expected);
    }
}
