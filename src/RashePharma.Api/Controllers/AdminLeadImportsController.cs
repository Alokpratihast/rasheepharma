using System.Security.Cryptography;
using System.Text;
using System.IO.Compression;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;

namespace RashePharma.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/leads/imports")]
public sealed class AdminLeadImportsController(ApplicationDbContext db) : ControllerBase
{
    private const long MaxUploadBytes = 10 * 1024 * 1024;
    private const int MaxImportedRows = 20_000;
    private static readonly string[] IncludedSheets = ["Export", "Product wise"];

    [HttpPost]
    [RequestSizeLimit(MaxUploadBytes)]
    [RequestFormLimits(MultipartBodyLengthLimit = MaxUploadBytes)]
    public async Task<IActionResult> CreatePreview(IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length is <= 0 or > MaxUploadBytes ||
            !string.Equals(Path.GetExtension(file.FileName), ".xlsx", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Choose a non-empty .xlsx file up to 10 MB." });

        await using var memory = new MemoryStream((int)file.Length);
        await file.CopyToAsync(memory, ct);
        var bytes = memory.ToArray();
        // Reject files without the OOXML ZIP signature before passing content to the workbook parser.
        if (bytes.Length < 4 || bytes[0] != 0x50 || bytes[1] != 0x4B)
            return BadRequest(new { message = "The file is not a valid Excel workbook." });

        var sha256 = Convert.ToHexString(SHA256.HashData(bytes));
        var prior = await db.LeadImportBatches.AsNoTracking()
            .FirstOrDefaultAsync(batch => batch.Sha256 == sha256 && batch.Status == "Confirmed", ct);
        if (prior is not null)
            return Conflict(new { message = "This exact workbook was already imported.", batchId = prior.Id });

        if (!HasSafeWorkbookArchive(bytes))
            return BadRequest(new { message = "The workbook archive exceeds the supported safety limits." });

        try
        {
            using var workbook = new XLWorkbook(new MemoryStream(bytes, writable: false));
            var missing = IncludedSheets.Where(name => !workbook.Worksheets.Any(sheet =>
                string.Equals(sheet.Name, name, StringComparison.OrdinalIgnoreCase))).ToArray();
            if (missing.Length > 0)
                return BadRequest(new { message = $"Required sheet(s) are missing: {string.Join(", ", missing)}." });

            var batch = new LeadImportBatch
            {
                FileName = Path.GetFileName(file.FileName)[..Math.Min(Path.GetFileName(file.FileName).Length, 260)],
                Sha256 = sha256
            };
            var seenEmails = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var total = 0;

            foreach (var sheetName in IncludedSheets)
            {
                var sheet = workbook.Worksheet(sheetName);
                var used = sheet.RangeUsed();
                if (used is null) continue;
                if (used.RowCount() > MaxImportedRows + 1 || used.ColumnCount() > 40)
                    return BadRequest(new { message = $"Sheet '{sheetName}' exceeds the import limits." });

                var headers = BuildHeaderMap(sheet, used.FirstRow().RowNumber(), used.LastColumn().ColumnNumber());
                var nameColumn = Find(headers, "NAME", "CONTACT NAME", "FULL NAME");
                var emailColumn = Find(headers, "MAIL ID", "EMAIL", "EMAIL ADDRESS");
                var phoneColumn = Find(headers, "NUMBER", "PHONE", "PHONE NUMBER", "MOBILE");
                if (nameColumn == 0 || (emailColumn == 0 && phoneColumn == 0))
                    return BadRequest(new { message = $"Sheet '{sheetName}' must include a name and an email or phone column." });

                var productColumn = Find(headers, "PRODUCT", "PRODUCT NAME", "PRODUCT INTEREST");
                var sentColumn = Find(headers, "MAIL SENT ON", "MAIL SENT ON(1)", "MAIL SENT ON(2)");
                var firstDataRow = used.FirstRow().RowNumber() + 1;
                for (var rowNumber = firstDataRow; rowNumber <= used.LastRow().RowNumber(); rowNumber++)
                {
                    var row = sheet.Row(rowNumber);
                    if (row.IsEmpty()) continue;
                    if (++total > MaxImportedRows)
                        return BadRequest(new { message = $"The workbook exceeds the {MaxImportedRows:N0} row import limit." });

                    var name = CellText(row, nameColumn, 200);
                    var email = emailColumn == 0 ? null : CellText(row, emailColumn, 320)?.Trim();
                    if (string.IsNullOrWhiteSpace(email)) email = null;
                    var normalized = email?.Trim().ToLowerInvariant();
                    var phone = phoneColumn == 0 ? null : CellText(row, phoneColumn, 40);
                    if (string.IsNullOrWhiteSpace(phone)) phone = null;
                    var product = productColumn == 0 ? null : CellText(row, productColumn, 1000);
                    var validationError = string.IsNullOrWhiteSpace(name) ? "Name is required." : null;
                    if (email is not null && !IsValidEmail(email)) validationError = "Email format is invalid.";
                    if (email is null && phone is null) validationError = "A valid email or phone number is required.";

                    var duplicate = normalized is not null && !seenEmails.Add(normalized);
                    batch.Rows.Add(new LeadImportRow
                    {
                        SheetName = sheetName,
                        SheetRow = rowNumber,
                        Name = name ?? string.Empty,
                        Email = email,
                        EmailNormalized = normalized,
                        Phone = phone,
                        ProductInterest = product,
                        HistoricalMailSentOn = sentColumn == 0 ? null : CellText(row, sentColumn, 200),
                        ValidationError = validationError,
                        IsDuplicate = duplicate
                    });
                }
            }

            batch.TotalRows = batch.Rows.Count;
            batch.ValidRows = batch.Rows.Count(row => row.ValidationError is null);
            batch.InvalidRows = batch.Rows.Count(row => row.ValidationError is not null);
            batch.DuplicateRows = batch.Rows.Count(row => row.IsDuplicate);
            await db.LeadImportBatches.AddAsync(batch, ct);
            await db.SaveChangesAsync(ct);
            return CreatedAtAction(nameof(GetPreview), new { id = batch.Id }, ToSummary(batch));
        }
        catch (ArgumentException)
        {
            return BadRequest(new { message = "The Excel workbook could not be read. Re-save it as .xlsx and retry." });
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            return BadRequest(new { message = "The Excel workbook is damaged or uses an unsupported format." });
        }
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetPreview(int id, [FromQuery] int page = 1, CancellationToken ct = default)
    {
        var batch = await db.LeadImportBatches.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id, ct);
        if (batch is null) return NotFound(new { message = "Import preview not found." });
        var rows = await db.LeadImportRows.AsNoTracking().Where(row => row.BatchId == id)
            .OrderBy(row => row.Id).Skip((Math.Clamp(page, 1, 1000) - 1) * 50).Take(50)
            .Select(row => new { row.SheetName, row.SheetRow, row.Name, row.Email, row.Phone,
                row.ProductInterest, row.HistoricalMailSentOn, row.ValidationError, row.IsDuplicate })
            .ToListAsync(ct);
        return Ok(new { batch = ToSummary(batch), page = Math.Clamp(page, 1, 1000), pageSize = 50, rows,
            permissionNotice = "Confirm that this workbook contains leads approved for email outreach. The historical 'Mail Sent On' field is not treated as consent evidence." });
    }

    [HttpPost("{id:int}/confirm")]
    public async Task<IActionResult> Confirm(int id, [FromBody] ConfirmLeadImportRequest request, CancellationToken ct)
    {
        if (!request.ConfirmEmailEligibility)
            return BadRequest(new { message = "Confirm that these contacts are eligible for email outreach before importing them." });

        await using var transaction = await db.Database.BeginTransactionAsync(ct);
        var batch = await db.LeadImportBatches.Include(item => item.Rows)
            .FirstOrDefaultAsync(item => item.Id == id, ct);
        if (batch is null) return NotFound(new { message = "Import preview not found." });
        if (batch.Status != "Preview") return Conflict(new { message = "This import batch was already confirmed." });

        var candidates = batch.Rows.Where(row => row.ValidationError is null).ToList();
        var emails = candidates.Where(row => row.EmailNormalized is not null)
            .Select(row => row.EmailNormalized!).Distinct().ToArray();
        var existing = await db.ExternalLeads.Where(lead => lead.EmailNormalized != null && emails.Contains(lead.EmailNormalized))
            .ToDictionaryAsync(lead => lead.EmailNormalized!, StringComparer.OrdinalIgnoreCase, ct);
        var phoneCandidates = candidates.Where(row => row.EmailNormalized is null && row.Phone is not null).ToList();
        var phoneValues = phoneCandidates.Select(row => row.Phone!).Distinct().ToArray();
        var phoneLeads = phoneValues.Length == 0 ? [] : await db.ExternalLeads
            .Where(lead => lead.EmailNormalized == null && lead.Phone != null && phoneValues.Contains(lead.Phone))
            .ToListAsync(ct);

        var confirmedAtUtc = DateTime.UtcNow;
        var permissionEvidence = $"Admin-confirmed for email outreach during import '{batch.FileName}' on {confirmedAtUtc:yyyy-MM-dd} UTC.";

        foreach (var row in candidates)
        {
            ExternalLead? lead = null;
            if (row.EmailNormalized is not null) existing.TryGetValue(row.EmailNormalized, out lead);
            else lead = phoneLeads.FirstOrDefault(item => item.Phone == row.Phone &&
                string.Equals(item.Name.Trim(), row.Name.Trim(), StringComparison.OrdinalIgnoreCase));

            if (lead is null)
            {
                lead = new ExternalLead { PermissionStatus = "Allowed", PermissionEvidence = permissionEvidence };
                await db.ExternalLeads.AddAsync(lead, ct);
                if (row.EmailNormalized is not null) existing[row.EmailNormalized] = lead;
                else phoneLeads.Add(lead);
            }
            else if (lead.PermissionStatus == "Unknown")
            {
                lead.PermissionStatus = "Allowed";
                lead.PermissionEvidence = permissionEvidence;
            }

            lead.Name = row.Name.Trim();
            lead.Email ??= row.Email;
            lead.EmailNormalized ??= row.EmailNormalized;
            lead.Phone ??= row.Phone;
            lead.ProductInterest = MergeText(lead.ProductInterest, row.ProductInterest);
            lead.HistoricalMailSentOn = MergeText(lead.HistoricalMailSentOn, row.HistoricalMailSentOn);
            lead.Source = MergeText(lead.Source == "Manual" ? null : lead.Source, row.SheetName) ?? row.SheetName;
            lead.UpdatedAtUtc = DateTime.UtcNow;
        }

        batch.Status = "Confirmed";
        batch.ImportedRows = candidates.Count;
        batch.ConfirmedAtUtc = confirmedAtUtc;
        db.LeadImportRows.RemoveRange(batch.Rows);
        await db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);
        return Ok(ToSummary(batch));
    }

    private static object ToSummary(LeadImportBatch batch) => new
    {
        batch.Id, batch.FileName, batch.Status, batch.TotalRows, batch.ValidRows,
        batch.InvalidRows, batch.DuplicateRows, batch.ImportedRows, batch.CreatedAtUtc
    };

    private static Dictionary<string, int> BuildHeaderMap(IXLWorksheet sheet, int headerRow, int lastColumn)
    {
        var result = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        for (var column = 1; column <= lastColumn; column++)
        {
            var key = NormalizeHeader(CellText(sheet.Row(headerRow), column, 100));
            if (!string.IsNullOrEmpty(key) && !result.ContainsKey(key)) result[key] = column;
        }
        return result;
    }

    private static int Find(Dictionary<string, int> headers, params string[] names)
    {
        foreach (var name in names)
            if (headers.TryGetValue(NormalizeHeader(name), out var column)) return column;
        return 0;
    }

    private static string NormalizeHeader(string? value) => new((value ?? string.Empty)
        .Where(char.IsLetterOrDigit).Select(char.ToUpperInvariant).ToArray());

    private static string? CellText(IXLRow row, int column, int maxLength)
    {
        if (column <= 0) return null;
        try
        {
            var text = row.Cell(column).GetFormattedString().Trim();
            return string.IsNullOrWhiteSpace(text) ? null : text[..Math.Min(text.Length, maxLength)];
        }
        catch { return null; }
    }

    private static bool IsValidEmail(string email)
    {
        if (email.Length > 320 || email.Any(char.IsWhiteSpace)) return false;
        try { return new System.Net.Mail.MailAddress(email).Address.Equals(email, StringComparison.OrdinalIgnoreCase); }
        catch { return false; }
    }

    private static bool HasSafeWorkbookArchive(byte[] bytes)
    {
        try
        {
            using var archive = new ZipArchive(new MemoryStream(bytes, writable: false), ZipArchiveMode.Read);
            if (archive.Entries.Count is 0 or > 300) return false;
            long expandedBytes = 0;
            foreach (var entry in archive.Entries)
            {
                if (entry.Length > 32 * 1024 * 1024) return false;
                expandedBytes += entry.Length;
                if (expandedBytes > 100 * 1024 * 1024) return false;
            }
            return archive.Entries.Any(entry => entry.FullName == "[Content_Types].xml") &&
                archive.Entries.Any(entry => entry.FullName.StartsWith("xl/", StringComparison.Ordinal));
        }
        catch (InvalidDataException)
        {
            return false;
        }
    }

    private static string? MergeText(string? current, string? next)
    {
        if (string.IsNullOrWhiteSpace(next)) return current;
        var parts = (current ?? string.Empty).Split("; ", StringSplitOptions.RemoveEmptyEntries);
        return parts.Contains(next, StringComparer.OrdinalIgnoreCase) ? current : string.Join("; ", parts.Append(next));
    }
}

public sealed class ConfirmLeadImportRequest
{
    public bool ConfirmEmailEligibility { get; set; }
}
