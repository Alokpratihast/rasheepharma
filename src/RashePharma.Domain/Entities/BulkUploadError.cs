namespace RashePharma.Domain.Entities;

public class BulkUploadError
{
    public int Id { get; set; }

    public int BulkUploadJobId { get; set; }

    public string SheetName { get; set; } = string.Empty;

    public int RowNumber { get; set; }

    public string? ProductSlug { get; set; }

    public string ErrorMessage { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public BulkUploadJob BulkUploadJob { get; set; } = null!;
}