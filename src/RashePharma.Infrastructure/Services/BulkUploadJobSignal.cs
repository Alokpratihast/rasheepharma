using System.Threading.Channels;
using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Infrastructure.Services;

/// <summary>
/// Process-local, non-blocking wake-up signal. Notifications are coalesced;
/// SQL remains the durable queue and periodic recovery handles missed signals.
/// </summary>
public sealed class BulkUploadJobSignal : IBulkUploadJobSignal
{
    private readonly Channel<byte> _channel = Channel.CreateBounded<byte>(
        new BoundedChannelOptions(1)
        {
            FullMode = BoundedChannelFullMode.DropWrite,
            SingleReader = true,
            SingleWriter = false
        });

    public void NotifyJobAvailable()
    {
        _channel.Writer.TryWrite(0);
    }

    public async Task<bool> WaitForJobAsync(
        TimeSpan timeout,
        CancellationToken cancellationToken)
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(timeout);

        try
        {
            if (!await _channel.Reader.WaitToReadAsync(timeoutCts.Token))
                return false;

            return _channel.Reader.TryRead(out _);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return false;
        }
    }
}
