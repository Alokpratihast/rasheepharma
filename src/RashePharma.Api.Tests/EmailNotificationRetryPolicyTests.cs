using RashePharma.Infrastructure.Services;
using Xunit;

namespace RashePharma.Api.Tests;

public sealed class EmailNotificationRetryPolicyTests
{
    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 5)]
    [InlineData(3, 15)]
    [InlineData(4, 30)]
    [InlineData(5, 30)]
    [InlineData(9, 30)]
    public void GetDelay_ReturnsConfiguredBackoffMinutes(int attemptCount, int expectedMinutes)
    {
        Assert.Equal(
            TimeSpan.FromMinutes(expectedMinutes),
            EmailNotificationRetryPolicy.GetDelay(attemptCount));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void GetDelay_WhenAttemptCountIsInvalid_Throws(int attemptCount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => EmailNotificationRetryPolicy.GetDelay(attemptCount));
    }
}
