using RashePharma.Infrastructure.Services;
using Xunit;

namespace RashePharma.Api.Tests;

public class BulkUploadJobSignalTests
{
    [Fact]
    public async Task WaitForJobAsync_WhenNotified_ReturnsTrue()
    {
        var signal = new BulkUploadJobSignal();
        signal.NotifyJobAvailable();

        var result = await signal.WaitForJobAsync(
            TimeSpan.FromSeconds(1),
            CancellationToken.None);

        Assert.True(result);
    }

    [Fact]
    public async Task WaitForJobAsync_WhenNoNotification_ReturnsFalseAfterTimeout()
    {
        var signal = new BulkUploadJobSignal();

        var result = await signal.WaitForJobAsync(
            TimeSpan.FromMilliseconds(50),
            CancellationToken.None);

        Assert.False(result);
    }

    [Fact]
    public async Task NotifyJobAvailable_MultipleNotificationsAreCoalesced()
    {
        var signal = new BulkUploadJobSignal();
        signal.NotifyJobAvailable();
        signal.NotifyJobAvailable();

        Assert.True(await signal.WaitForJobAsync(
            TimeSpan.FromSeconds(1),
            CancellationToken.None));

        Assert.False(await signal.WaitForJobAsync(
            TimeSpan.FromMilliseconds(50),
            CancellationToken.None));
    }

    [Fact]
    public async Task WaitForJobAsync_WhenCallerCancels_ThrowsOperationCanceledException()
    {
        var signal = new BulkUploadJobSignal();
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => signal.WaitForJobAsync(
                TimeSpan.FromSeconds(1),
                cancellation.Token));
    }
}
