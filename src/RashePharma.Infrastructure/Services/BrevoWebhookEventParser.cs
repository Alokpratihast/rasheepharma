using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using RashePharma.Domain.Enums.Email;

namespace RashePharma.Infrastructure.Services;

public sealed record BrevoWebhookEvent(
    string Email,
    string EventName,
    string? ProviderMessageId,
    string? CustomCorrelation,
    string ProviderEventId,
    string? Reason,
    DateTime OccurredAtUtc,
    EmailDeliveryStatus? DeliveryStatus,
    string? SuppressionType,
    string RawPayloadJson);

/// <summary>Parses and normalizes Brevo transactional event payloads.</summary>
public static class BrevoWebhookEventParser
{
    public static bool TryParse(JsonElement value, out BrevoWebhookEvent? parsed)
    {
        parsed = null;
        if (value.ValueKind != JsonValueKind.Object)
            return false;

        var email = ReadString(value, "email");
        var eventName = ReadString(value, "event");
        if (string.IsNullOrWhiteSpace(email) ||
            email.Length > 320 ||
            !email.Contains('@', StringComparison.Ordinal) ||
            string.IsNullOrWhiteSpace(eventName))
            return false;

        var normalizedEvent = eventName.Trim().ToLowerInvariant().Replace('-', '_');
        EmailDeliveryStatus? status = normalizedEvent switch
        {
            "request" or "sent" => EmailDeliveryStatus.Accepted,
            "delivered" => EmailDeliveryStatus.Delivered,
            "opened" or "unique_opened" or "uniqueopened" => EmailDeliveryStatus.Opened,
            "click" or "clicked" => EmailDeliveryStatus.Clicked,
            "hard_bounce" or "hardbounce" or "soft_bounce" or "softbounce" or "bounce" or "blocked" => EmailDeliveryStatus.Bounced,
            "spam" or "complaint" => EmailDeliveryStatus.Complained,
            "unsubscribed" => EmailDeliveryStatus.Unsubscribed,
            _ => null
        };

        var suppression = normalizedEvent switch
        {
            "hard_bounce" or "hardbounce" => "HardBounce",
            "spam" or "complaint" => "Complaint",
            "unsubscribed" => "Unsubscribed",
            _ => null
        };
        var messageId = ReadString(value, "message-id") ?? ReadString(value, "messageId");
        var explicitEventId = ReadString(value, "id");
        var custom = ReadString(value, "X-Mailin-custom") ??
                     ReadString(value, "x-mailin-custom") ??
                     ReadString(value, "custom");
        var occurredAt = ReadOccurredAt(value);
        var raw = value.GetRawText();
        var eventId = string.IsNullOrWhiteSpace(explicitEventId)
            ? Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(
                string.Join('|', email.Trim().ToLowerInvariant(), normalizedEvent,
                    messageId ?? string.Empty, occurredAt.Ticks.ToString(CultureInfo.InvariantCulture),
                    ReadString(value, "link") ?? string.Empty))))
            : explicitEventId[..Math.Min(explicitEventId.Length, 200)];

        parsed = new BrevoWebhookEvent(
            email.Trim(), normalizedEvent, messageId, custom, eventId,
            ReadString(value, "reason"), occurredAt, status, suppression, raw);
        return true;
    }

    private static string? ReadString(JsonElement value, string name)
    {
        foreach (var property in value.EnumerateObject())
        {
            if (!string.Equals(property.Name, name, StringComparison.OrdinalIgnoreCase))
                continue;
            if (property.Value.ValueKind == JsonValueKind.String)
                return property.Value.GetString();
            if (property.Value.ValueKind == JsonValueKind.Number)
                return property.Value.GetRawText();
        }
        return null;
    }

    private static DateTime ReadOccurredAt(JsonElement value)
    {
        var raw = ReadString(value, "ts_event") ?? ReadString(value, "ts") ??
                  ReadString(value, "date");
        if (long.TryParse(raw, NumberStyles.Integer, CultureInfo.InvariantCulture, out var unix))
        {
            try
            {
                // Brevo's numeric event timestamps are Unix seconds; tolerate milliseconds.
                return DateTimeOffset.FromUnixTimeMilliseconds(unix > 10_000_000_000
                        ? unix
                        : checked(unix * 1000))
                    .UtcDateTime;
            }
            catch (ArgumentOutOfRangeException) { }
            catch (OverflowException) { }
        }

        if (DateTimeOffset.TryParse(raw, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal, out var date))
            return date.UtcDateTime;
        return DateTime.UtcNow;
    }
}
