namespace RashePharma.Domain.Entities;

public sealed class LeadImportBatch
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Sha256 { get; set; } = string.Empty;
    public string Status { get; set; } = "Preview";
    public int TotalRows { get; set; }
    public int ValidRows { get; set; }
    public int InvalidRows { get; set; }
    public int DuplicateRows { get; set; }
    public int ImportedRows { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ConfirmedAtUtc { get; set; }
    public ICollection<LeadImportRow> Rows { get; set; } = new List<LeadImportRow>();
}
