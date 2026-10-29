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
[Route("api/admin/lead-campaigns")]
public sealed class AdminLeadCampaignsController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetRecent(CancellationToken ct)
        => Ok(await db.LeadOutreachCampaigns.AsNoTracking().OrderByDescending(campaign => campaign.CreatedAtUtc)
            .Take(100).Select(campaign => new
            {
                campaign.Id,
                campaign.Name,
                campaign.Subject,
                Status = campaign.Recipients.Any(row => row.Status == "Pending" || row.Status == "Processing")
                    ? "Queued"
                    : campaign.Recipients.Any(row => row.Status == "Failed") ? "CompletedWithErrors" : "Completed",
                campaign.RecipientCount,
                SentCount = campaign.Recipients.Count(row => row.Status == "Sent"),
                FailedCount = campaign.Recipients.Count(row => row.Status == "Failed"),
                campaign.CreatedAtUtc,
                campaign.QueuedAtUtc,
                campaign.CompletedAtUtc
            }).ToListAsync(ct));

    [HttpPost("send")]
    public async Task<IActionResult> Queue(QueueLeadCampaignRequest request, CancellationToken ct)
    {
        if (request.LeadIds is null || request.LeadIds.Count == 0 || request.LeadIds.Count > 5000)
            return BadRequest(new { message = "Select between 1 and 5,000 leads." });
        var ids = request.LeadIds.Distinct().ToArray();
        if (!request.ConfirmSend)
            return BadRequest(new { message = "Confirm the send action before queueing this campaign." });

        var eligible = await db.ExternalLeads.Where(lead => ids.Contains(lead.Id) &&
                lead.Email != null && lead.PermissionStatus == "Allowed" && !lead.IsSuppressed)
            .Select(lead => new { lead.Id }).ToListAsync(ct);
        if (eligible.Count == 0)
            return BadRequest(new { message = "No selected lead has both verified permission and an active email address." });

        var campaign = new LeadOutreachCampaign
        {
            Name = request.Name.Trim(),
            Subject = request.Subject.Trim(),
            BodyText = request.BodyText.Trim(),
            Status = "Queued",
            RecipientCount = eligible.Count,
            CreatedBy = User.FindFirstValue(ClaimTypes.Email) ?? User.Identity?.Name ?? "admin",
            QueuedAtUtc = DateTime.UtcNow
        };
        foreach (var lead in eligible)
        {
            campaign.Recipients.Add(new LeadOutreachRecipient
            {
                LeadId = lead.Id,
                Status = "Pending"
            });
        }
        await db.LeadOutreachCampaigns.AddAsync(campaign, ct);
        await db.SaveChangesAsync(ct);
        return Accepted(new { campaign.Id, campaign.Status, campaign.RecipientCount,
            excludedCount = ids.Length - eligible.Count,
            message = "Campaign queued. Only selected leads with Allowed permission are included." });
    }
}

public sealed class QueueLeadCampaignRequest
{
    [Required, StringLength(160)] public string Name { get; set; } = string.Empty;
    [Required, StringLength(200)] public string Subject { get; set; } = string.Empty;
    [Required, StringLength(12000)] public string BodyText { get; set; } = string.Empty;
    [Required, MinLength(1), MaxLength(5000)] public List<int> LeadIds { get; set; } = [];
    public bool ConfirmSend { get; set; }
}
