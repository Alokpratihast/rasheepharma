using RashePharma.Infrastructure.Services;
using Xunit;

namespace RashePharma.Api.Tests;

public sealed class LeadOutreachResponseTokenTests
{
    private const string Secret = "test-signing-secret-for-response-token";

    [Fact]
    public void ValidTokenIsBoundToRecipientAndLeadAndExpires()
    {
        var now = DateTime.UtcNow;
        var token = LeadOutreachResponseToken.Create(24, 91, now.AddDays(30), Secret);

        Assert.True(LeadOutreachResponseToken.TryRead(token, Secret, now, out var recipientId, out var leadId));
        Assert.Equal(24, recipientId);
        Assert.Equal(91, leadId);
        Assert.False(LeadOutreachResponseToken.TryRead(token, Secret, now.AddDays(31), out _, out _));
    }

    [Fact]
    public void AlteredTokenAndWrongSecretAreRejected()
    {
        var now = DateTime.UtcNow;
        var token = LeadOutreachResponseToken.Create(24, 91, now.AddDays(1), Secret);
        var last = token[^1] == '0' ? '1' : '0';

        Assert.False(LeadOutreachResponseToken.TryRead(token[..^1] + last, Secret, now, out _, out _));
        Assert.False(LeadOutreachResponseToken.TryRead(token, "wrong-secret", now, out _, out _));
    }

    [Theory]
    [InlineData(0, 91)]
    [InlineData(24, 0)]
    public void InvalidIdentifiersAreRejected(int recipientId, int leadId)
    {
        var now = DateTime.UtcNow;
        var token = LeadOutreachResponseToken.Create(recipientId, leadId, now.AddDays(1), Secret);
        Assert.False(LeadOutreachResponseToken.TryRead(token, Secret, now, out _, out _));
    }
}
