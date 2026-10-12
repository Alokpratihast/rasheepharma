namespace RashePharma.Domain.Enums;

public enum BulkUploadStatus
{
    Pending = 0,
    Processing = 1,
    Completed = 2,
    CompletedWithErrors = 3,
    Failed = 4
}