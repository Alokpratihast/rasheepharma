namespace RashePharma.Domain.Entities.Email;

public static class EmailOutboxStatus
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string Published = "Published";
    public const string RetryScheduled = "RetryScheduled";
    public const string DeadLettered = "DeadLettered";
}