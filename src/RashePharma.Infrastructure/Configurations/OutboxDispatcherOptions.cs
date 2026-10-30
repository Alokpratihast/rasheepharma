
namespace RashePharma.Infrastructure.Configurations;

public sealed class OutboxDispatcherOptions
{
    public const string SectionName = "OutboxDispatcher";

    public int BatchSize { get; set; } = 20;

    public int PollingIntervalSeconds { get; set; } = 5;

    public int LeaseDurationSeconds { get; set; } = 60;

    public int MaxAttempts { get; set; } = 5;

    public int RetryBaseDelaySeconds { get; set; } = 30;

    public int RetryMaxDelaySeconds { get; set; } = 3600;
}
