using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;

namespace RashePharma.Api.Controllers;

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/leads")]
public sealed class AdminLeadsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPage(
        [FromQuery] string? search = null,
        [FromQuery] string permission = "All",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken ct = default)
    {
        page = Math.Clamp(page, 1, 100_000);
        pageSize = Math.Clamp(pageSize, 10, 100);
        var query = db.ExternalLeads.AsNoTracking();
        if (permission is "Unknown" or "Allowed" or "Blocked")
            query = query.Where(lead => lead.PermissionStatus == permission);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim()[..Math.Min(search.Trim().Length, 100)];
            query = query.Where(lead => lead.Name.Contains(term) ||
                (lead.Email != null && lead.Email.Contains(term)) ||
                (lead.Company != null && lead.Company.Contains(term)) ||
                (lead.ProductInterest != null && lead.ProductInterest.Contains(term)));
        }

        var total = await query.CountAsync(ct);
        var leads = await query.OrderByDescending(lead => lead.UpdatedAtUtc)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(lead => new
            {
                lead.Id, lead.Name, lead.Email, lead.Phone, lead.Company, lead.Country,
                lead.BusinessType, lead.ProductInterest, lead.Source, lead.PermissionStatus,
                lead.PermissionEvidence, lead.HistoricalMailSentOn, lead.IsSuppressed,
                lead.CreatedAtUtc, lead.UpdatedAtUtc,
                CanReceiveMarketingEmail = lead.PermissionStatus == "Allowed" && !lead.IsSuppressed && lead.Email != null
            }).ToListAsync(ct);

        var counts = await db.ExternalLeads.AsNoTracking().GroupBy(_ => 1).Select(group => new
        {
            Total = group.Count(),
            Allowed = group.Count(lead => lead.PermissionStatus == "Allowed"),
            Unknown = group.Count(lead => lead.PermissionStatus == "Unknown"),
            UnknownEmail = group.Count(lead => lead.PermissionStatus == "Unknown" && !lead.IsSuppressed && lead.Email != null),
            Suppressed = group.Count(lead => lead.IsSuppressed)
        }).FirstOrDefaultAsync(ct);
        return Ok(new { leads, page, pageSize, total, counts = counts ?? new { Total = 0, Allowed = 0, Unknown = 0, UnknownEmail = 0, Suppressed = 0 } });
    }

    [HttpPut("{id:int}/permission")]
    public async Task<IActionResult> UpdatePermission(int id, UpdateLeadPermissionRequest request, CancellationToken ct)
    {
        if (request.PermissionStatus is not ("Unknown" or "Allowed" or "Blocked"))
            return BadRequest(new { message = "Permission status must be Unknown, Allowed, or Blocked." });
        if (request.PermissionStatus == "Allowed" && string.IsNullOrWhiteSpace(request.Evidence))
            return BadRequest(new { message = "Record the permission source before enabling email outreach." });

        var lead = await db.ExternalLeads.FirstOrDefaultAsync(item => item.Id == id, ct);
        if (lead is null) return NotFound(new { message = "Lead not found." });
        var before = lead.PermissionStatus;
        lead.PermissionStatus = request.PermissionStatus;
        lead.PermissionEvidence = request.PermissionStatus == "Allowed" ? request.Evidence!.Trim() : null;
        lead.UpdatedAtUtc = DateTime.UtcNow;
        db.LeadActivities.Add(new LeadActivity
        {
            LeadId = lead.Id,
            Type = "PermissionChanged",
            Details = $"Permission changed from {before} to {lead.PermissionStatus}. Evidence: {lead.PermissionEvidence ?? "not provided"}",
            Actor = Actor()
        });
        await db.SaveChangesAsync(ct);
        return Ok(new { lead.Id, lead.PermissionStatus, lead.PermissionEvidence, lead.IsSuppressed,
            CanReceiveMarketingEmail = lead.PermissionStatus == "Allowed" && !lead.IsSuppressed && lead.Email is not null });
    }

    [HttpPost("confirm-unknown")]
    public async Task<IActionResult> ConfirmUnknownLeads(ConfirmUnknownLeadsRequest request, CancellationToken ct)
    {
        if (!request.ConfirmEmailEligibility)
            return BadRequest(new { message = "Confirm that the unknown email leads are eligible for outreach." });

        var leads = await db.ExternalLeads
            .Where(lead => lead.PermissionStatus == "Unknown" && !lead.IsSuppressed && lead.Email != null)
            .ToListAsync(ct);
        var confirmedAtUtc = DateTime.UtcNow;
        var evidence = $"Admin-confirmed for email outreach in bulk on {confirmedAtUtc:yyyy-MM-dd} UTC.";
        foreach (var lead in leads)
        {
            lead.PermissionStatus = "Allowed";
            lead.PermissionEvidence = evidence;
            lead.UpdatedAtUtc = confirmedAtUtc;
        }

        await db.SaveChangesAsync(ct);
        return Ok(new { updatedCount = leads.Count });
    }

    private string Actor() => User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name ?? "admin";
}

public sealed class UpdateLeadPermissionRequest
{
    [Required, StringLength(20)] public string PermissionStatus { get; set; } = "Unknown";
    [StringLength(500)] public string? Evidence { get; set; }
}

public sealed class ConfirmUnknownLeadsRequest
{
    [Required] public bool ConfirmEmailEligibility { get; set; }
}
