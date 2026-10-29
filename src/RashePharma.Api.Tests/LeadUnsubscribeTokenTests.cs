using RashePharma.Infrastructure.Services;
using Xunit;

namespace RashePharma.Api.Tests;

public sealed class LeadUnsubscribeTokenTests
{
    private const string Secret = "test-signing-secret-for-unit-tests";

    [Fact]
    public void CreatedTokenCanBeReadWithMatchingIds()
    {
        var token = LeadUnsubscribeToken.Create(42, 17, Secret);

        Assert.True(LeadUnsubscribeToken.TryRead(token, Secret, out var recipientId, out var leadId));
        Assert.Equal(42, recipientId);
        Assert.Equal(17, leadId);
    }

    [Theory]
    [InlineData("wrong-signing-secret")]
    [InlineData("tampered")]
    public void InvalidSignatureOrTokenIsRejected(string alteration)
    {
        var token = LeadUnsubscribeToken.Create(42, 17, Secret);
        var suppliedToken = alteration == "tampered" ? token[..^1] + (token[^1] == '0' ? "1" : "0") : token;
        var suppliedSecret = alteration == "wrong-signing-secret" ? alteration : Secret;

        Assert.False(LeadUnsubscribeToken.TryRead(suppliedToken, suppliedSecret, out _, out _));
    }
}
