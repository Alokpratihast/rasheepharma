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
                campaign.AttachBrochure,
                Status = campaign.Recipients.Any(row => row.Status == "Pending" || row.Status == "Processing" || row.Status == "Queued")
                    ? "Queued"
                    : campaign.Recipients.Any(row => row.Status == "Failed") ? "CompletedWithErrors" : "Completed",
                campaign.RecipientCount,
                SentCount = campaign.Recipients.Count(row => row.Status == "Sent"),
                FailedCount = campaign.Recipients.Count(row => row.Status == "Failed"),
                RespondedCount = db.LeadOutreachResponses.Count(response => response.Recipient.CampaignId == campaign.Id),
                campaign.CreatedAtUtc,
                campaign.QueuedAtUtc,
                campaign.CompletedAtUtc
            }).ToListAsync(ct));

    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] string? search = null,
        [FromQuery] string status = "All",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken ct = default)
    {
        page = Math.Clamp(page, 1, 100_000);
        pageSize = Math.Clamp(pageSize, 10, 100);
        var query = db.LeadOutreachCampaigns.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            term = term[..Math.Min(term.Length, 100)];
            query = query.Where(campaign => campaign.Name.Contains(term) || campaign.Subject.Contains(term));
        }
        if (status == "Queued")
            query = query.Where(campaign => campaign.Recipients.Any(row =>
                row.Status == "Pending" || row.Status == "Processing" || row.Status == "Queued"));
        else if (status == "CompletedWithErrors")
            query = query.Where(campaign =>
                !campaign.Recipients.Any(row => row.Status == "Pending" || row.Status == "Processing" || row.Status == "Queued") &&
                campaign.Recipients.Any(row => row.Status == "Failed"));
        else if (status == "Completed")
            query = query.Where(campaign =>
                !campaign.Recipients.Any(row => row.Status == "Pending" || row.Status == "Processing" || row.Status == "Queued") &&
                !campaign.Recipients.Any(row => row.Status == "Failed"));

        var total = await query.CountAsync(ct);
        var campaigns = await query.OrderByDescending(campaign => campaign.CreatedAtUtc)
            .ThenByDescending(campaign => campaign.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(campaign => new
            {
                campaign.Id,
                campaign.Name,
                campaign.Subject,
                campaign.AttachBrochure,
                Status = campaign.Recipients.Any(row => row.Status == "Pending" || row.Status == "Processing" || row.Status == "Queued")
                    ? "Queued"
                    : campaign.Recipients.Any(row => row.Status == "Failed") ? "CompletedWithErrors" : "Completed",
                campaign.RecipientCount,
                SentCount = campaign.Recipients.Count(row => row.Status == "Sent"),
                FailedCount = campaign.Recipients.Count(row => row.Status == "Failed"),
                RespondedCount = db.LeadOutreachResponses.Count(response => response.Recipient.CampaignId == campaign.Id),
                campaign.CreatedAtUtc,
                campaign.QueuedAtUtc,
                campaign.CompletedAtUtc
            }).ToListAsync(ct);

        return Ok(new { campaigns, page, pageSize, total });
    }

    [HttpGet("{campaignId:int}")]
    public async Task<IActionResult> GetCampaignDetail(
        int campaignId,
        [FromQuery] string? search = null,
        [FromQuery] string status = "All",
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken ct = default)
    {
        page = Math.Clamp(page, 1, 100_000);
        pageSize = Math.Clamp(pageSize, 10, 100);

        var campaign = await db.LeadOutreachCampaigns.AsNoTracking()
            .Where(item => item.Id == campaignId)
            .Select(item => new
            {
                item.Id,
                item.Name,
                item.Subject,
                item.BodyText,
                item.AttachBrochure,
                Status = item.Recipients.Any(row => row.Status == "Pending" || row.Status == "Processing" || row.Status == "Queued")
                    ? "Queued"
                    : item.Recipients.Any(row => row.Status == "Failed") ? "CompletedWithErrors" : "Completed",
                item.RecipientCount,
                SentCount = item.Recipients.Count(row => row.Status == "Sent"),
                FailedCount = item.Recipients.Count(row => row.Status == "Failed"),
                RespondedCount = db.LeadOutreachResponses.Count(response => response.Recipient.CampaignId == item.Id),
                item.CreatedAtUtc,
                item.QueuedAtUtc,
                item.CompletedAtUtc
            }).FirstOrDefaultAsync(ct);
        if (campaign is null) return NotFound(new { message = "Campaign not found." });

        var recipientsQuery = db.LeadOutreachRecipients.AsNoTracking()
            .Where(recipient => recipient.CampaignId == campaignId);
        var statusCounts = await recipientsQuery.GroupBy(recipient => recipient.Status)
            .Select(group => new { Status = group.Key, Count = group.Count() })
            .ToListAsync(ct);

        if (status is "Pending" or "Processing" or "Queued" or "Sent" or "Failed" or "Skipped")
            recipientsQuery = recipientsQuery.Where(recipient => recipient.Status == status);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            term = term[..Math.Min(term.Length, 100)];
            recipientsQuery = recipientsQuery.Where(recipient => recipient.Lead.Name.Contains(term) ||
                (recipient.Lead.Email != null && recipient.Lead.Email.Contains(term)) ||
                (recipient.Lead.ProductInterest != null && recipient.Lead.ProductInterest.Contains(term)));
        }

        var total = await recipientsQuery.CountAsync(ct);
        var recipients = await recipientsQuery.OrderByDescending(recipient => recipient.CreatedAtUtc)
            .ThenBy(recipient => recipient.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(recipient => new
            {
                recipient.Id,
                Name = recipient.Lead.Name,
                Email = recipient.Lead.Email,
                ProductInterest = recipient.Lead.ProductInterest,
                recipient.Status,
                recipient.AttemptCount,
                recipient.ErrorMessage,
                recipient.CreatedAtUtc,
                recipient.SentAtUtc
            }).ToListAsync(ct);

        return Ok(new { campaign, recipients, page, pageSize, total, statusCounts });
    }

    [HttpGet("{campaignId:int}/responses")]
    public async Task<IActionResult> GetResponses(int campaignId, CancellationToken ct)
    {
        var campaignExists = await db.LeadOutreachCampaigns.AsNoTracking().AnyAsync(campaign => campaign.Id == campaignId, ct);
        if (!campaignExists) return NotFound(new { message = "Campaign not found." });

        var responses = await db.LeadOutreachResponses.AsNoTracking()
            .Where(response => response.Recipient.CampaignId == campaignId)
            .OrderByDescending(response => response.SubmittedAtUtc)
            .Take(500)
            .Select(response => new
            {
                response.Id,
                response.Recipient.Lead.Name,
                response.Recipient.Lead.Email,
                response.Message,
                response.SubmittedAtUtc
            }).ToListAsync(ct);
        return Ok(responses);
    }

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
            AttachBrochure = request.AttachBrochure,
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
    public bool AttachBrochure { get; set; }
    [Required, MinLength(1), MaxLength(5000)] public List<int> LeadIds { get; set; } = [];
    public bool ConfirmSend { get; set; }
}
