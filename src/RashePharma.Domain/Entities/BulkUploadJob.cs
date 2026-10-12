using RashePharma.Domain.Enums;

namespace RashePharma.Domain.Entities;

public class BulkUploadJob
{
    public int Id { get; set; }

    public string FileName { get; set; } = string.Empty;

    public BulkUploadStatus Status { get; set; }
        = BulkUploadStatus.Pending;

    public int TotalRecords { get; set; }

    public int ProcessedRecords { get; set; }

    public int SuccessCount { get; set; }

    public int ErrorCount { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public ICollection<BulkUploadError> Errors { get; set; }
        = new List<BulkUploadError>();

    public ICollection<BulkUploadFile> Files { get; set; }
    = new List<BulkUploadFile>();
}