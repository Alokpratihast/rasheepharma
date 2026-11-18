namespace RashePharma.Domain.Entities.BulkUpload;

public static class BulkUploadDispatchStatus
{
    public const string Pending = "Pending";
    public const string Processing = "Processing";
    public const string Published = "Published";
    public const string RetryScheduled = "RetryScheduled";
    public const string DeadLettered = "DeadLettered";
}
