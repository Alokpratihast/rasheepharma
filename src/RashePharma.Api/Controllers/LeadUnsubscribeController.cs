using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;
using RashePharma.Infrastructure.Services;

namespace RashePharma.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/lead-outreach")]
public sealed class LeadUnsubscribeController(ApplicationDbContext db, IConfiguration configuration) : ControllerBase
{
    [HttpPost("unsubscribe")]
    [EnableRateLimiting("lead-unsubscribe")]
    public async Task<IActionResult> Unsubscribe(UnsubscribeLeadRequest request, CancellationToken ct)
    {
        var secret = configuration["Jwt:Key"];
        if (string.IsNullOrWhiteSpace(secret) ||
            !LeadUnsubscribeToken.TryRead(request.Token, secret, out var recipientId, out var leadId))
            return Ok(new { message = "If this link is valid, the address has been unsubscribed." });
        var recipient = await db.LeadOutreachRecipients.Include(item => item.Lead)
            .FirstOrDefaultAsync(item => item.Id == recipientId && item.LeadId == leadId, ct);
        // Same response for unknown tokens avoids leaking whether an address is on the list.
        if (recipient is null) return Ok(new { message = "If this link is valid, the address has been unsubscribed." });
        if (!recipient.Lead.IsSuppressed)
        {
            recipient.Lead.IsSuppressed = true;
            recipient.Lead.SuppressedAtUtc = DateTime.UtcNow;
            recipient.Lead.UpdatedAtUtc = DateTime.UtcNow;
            db.LeadActivities.Add(new LeadActivity
            {
                LeadId = recipient.LeadId,
                Type = "Unsubscribed",
                Details = "Recipient opted out through an outreach email.",
                Actor = "recipient"
            });
            await db.SaveChangesAsync(ct);
        }
        return Ok(new { message = "If this link is valid, the address has been unsubscribed." });
    }
}

public sealed class UnsubscribeLeadRequest
{
    [Required, StringLength(200, MinimumLength = 1)] public string Token { get; set; } = string.Empty;
}
