namespace RashePharma.Infrastructure.Services;

/// <summary>
/// Durable retry backoff for failed email notifications.
/// The SQL claim query uses these cutoffs during each recovery sweep.
/// </summary>
public static class EmailNotificationRetryPolicy
{
    public static TimeSpan GetDelay(int attemptCount) => attemptCount switch
    {
        1 => TimeSpan.FromMinutes(1),
        2 => TimeSpan.FromMinutes(5),
        3 => TimeSpan.FromMinutes(15),
        >= 4 => TimeSpan.FromMinutes(30),
        _ => throw new ArgumentOutOfRangeException(
            nameof(attemptCount),
            "Attempt count must be at least one.")
    };
}
