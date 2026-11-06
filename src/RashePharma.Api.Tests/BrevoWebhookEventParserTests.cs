using System.Text.Json;
using RashePharma.Domain.Enums.Email;
using RashePharma.Infrastructure.Services;
using Xunit;

namespace RashePharma.Api.Tests;

public sealed class BrevoWebhookEventParserTests
{
    [Theory]
    [InlineData("delivered", EmailDeliveryStatus.Delivered, null)]
    [InlineData("request", EmailDeliveryStatus.Accepted, null)]
    [InlineData("opened", EmailDeliveryStatus.Opened, null)]
    [InlineData("uniqueOpened", EmailDeliveryStatus.Opened, null)]
    [InlineData("click", EmailDeliveryStatus.Clicked, null)]
    [InlineData("hard_bounce", EmailDeliveryStatus.Bounced, "HardBounce")]
    [InlineData("hardBounce", EmailDeliveryStatus.Bounced, "HardBounce")]
    [InlineData("softBounce", EmailDeliveryStatus.Bounced, null)]
    [InlineData("spam", EmailDeliveryStatus.Complained, "Complaint")]
    [InlineData("unsubscribed", EmailDeliveryStatus.Unsubscribed, "Unsubscribed")]
    public void TryParse_MapsBrevoEvents(
        string eventName,
        EmailDeliveryStatus expectedStatus,
        string? expectedSuppression)
    {
        var payload = JsonSerializer.Serialize(new
        {
            email = "lead@example.com",
            @event = eventName,
            XMailinCustom = "2fba39f0-6bc4-4b21-a8cb-29be8702e1f0",
            ts_event = 1793872800
        }).Replace("XMailinCustom", "X-Mailin-custom", StringComparison.Ordinal);
        using var document = JsonDocument.Parse(payload);

        var parsed = BrevoWebhookEventParser.TryParse(document.RootElement, out var result);

        Assert.True(parsed);
        Assert.NotNull(result);
        Assert.Equal(expectedStatus, result.DeliveryStatus);
        Assert.Equal(expectedSuppression, result.SuppressionType);
        Assert.Equal("2fba39f0-6bc4-4b21-a8cb-29be8702e1f0", result.CustomCorrelation);
        Assert.Equal(DateTimeKind.Utc, result.OccurredAtUtc.Kind);
    }

    [Fact]
    public void TryParse_UsesDeterministicIdWhenProviderOmitsEventId()
    {
        const string payload = """{"email":"Lead@Example.com","event":"opened","message-id":"abc","date":"2026-11-05T12:30:00Z"}""";
        using var first = JsonDocument.Parse(payload);
        using var second = JsonDocument.Parse(payload);

        Assert.True(BrevoWebhookEventParser.TryParse(first.RootElement, out var a));
        Assert.True(BrevoWebhookEventParser.TryParse(second.RootElement, out var b));
        Assert.Equal(a!.ProviderEventId, b!.ProviderEventId);
        Assert.Equal(64, a.ProviderEventId.Length);
    }

    [Theory]
    [InlineData("{}")]
    [InlineData("{\"email\":\"invalid\",\"event\":\"opened\"}")]
    [InlineData("{\"email\":\"lead@example.com\"}")]
    public void TryParse_RejectsMalformedOrUncorrelatablePayload(string payload)
    {
        using var document = JsonDocument.Parse(payload);
        Assert.False(BrevoWebhookEventParser.TryParse(document.RootElement, out _));
    }
}
