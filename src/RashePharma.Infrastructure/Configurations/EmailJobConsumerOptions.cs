namespace RashePharma.Infrastructure.Configurations;

public sealed class EmailJobConsumerOptions
{
    public const string SectionName = "EmailJobConsumer";

    public int MaxConcurrentCalls { get; set; } = 4;

    public int ProcessingLeaseMinutes { get; set; } = 4;
}
