namespace RashePharma.Domain.Entities;

public sealed class LeadImportRow
{
    public int Id { get; set; }
    public int BatchId { get; set; }
    public string SheetName { get; set; } = string.Empty;
    public int SheetRow { get; set; }
    public string? Email { get; set; }
    public string? EmailNormalized { get; set; }
    public string? Phone { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? ProductInterest { get; set; }
    public string? HistoricalMailSentOn { get; set; }
    public string? ValidationError { get; set; }
    public bool IsDuplicate { get; set; }
    public LeadImportBatch Batch { get; set; } = null!;
}
