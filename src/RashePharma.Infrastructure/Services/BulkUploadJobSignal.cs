using System.Threading.Channels;
using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Infrastructure.Services;

/// <summary>
/// Process-local, non-blocking wake-up signal. Channel notifications are
/// intentionally coalesced: the worker drains all claimable SQL jobs after
/// each wake-up, so one signal is sufficient for any number of queued jobs.
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
        // A full channel already represents a pending wake-up.
        _channel.Writer.TryWrite(0);
    }

    public async ValueTask WaitForJobAsync(CancellationToken cancellationToken)
    {
        // ReadAsync returns the consumed byte; the worker only needs the wake-up.
        await _channel.Reader.ReadAsync(cancellationToken);
    }
}
