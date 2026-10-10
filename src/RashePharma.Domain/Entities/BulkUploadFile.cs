namespace RashePharma.Domain.Entities;

public class BulkUploadFile
{
    public int Id { get; set; }

    public int BulkUploadJobId { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public string BlobName { get; set; } = string.Empty;

    public string FileUrl { get; set; } = string.Empty;

    public string FileType { get; set; } = string.Empty;

    public string? ContentType { get; set; }

    public long FileSize { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public BulkUploadJob BulkUploadJob { get; set; } = null!;


    
}