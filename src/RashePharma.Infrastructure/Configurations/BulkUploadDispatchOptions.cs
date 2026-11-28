namespace RashePharma.Infrastructure.Configurations;
public sealed class BulkUploadDispatchOptions
{
 public const string SectionName = "BulkUploadDispatch";
 public int BatchSize { get; set; } = 20;
 public int LeaseDurationSeconds { get; set; } = 60;
 public int MaxAttempts { get; set; } = 8;
 public int RetryBaseDelaySeconds { get; set; } = 15;
 public int RetryMaxDelaySeconds { get; set; } = 900;
 public int RecoveryIntervalSeconds { get; set; } = 60;
}
