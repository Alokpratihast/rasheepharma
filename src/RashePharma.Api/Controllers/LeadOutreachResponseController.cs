using System.ComponentModel.DataAnnotations;
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
[Route("api/lead-outreach/responses")]
public sealed class LeadOutreachResponseController(ApplicationDbContext db, IConfiguration configuration) : ControllerBase
{
    [HttpGet("validate")]
    [EnableRateLimiting("lead-unsubscribe")]
    public async Task<IActionResult> Validate([FromQuery] string token, CancellationToken ct)
    {
        if (!TryReadToken(token, out var recipientId, out var leadId)) return BadRequest(new { message = "This response link is invalid or expired." });
        var valid = await db.LeadOutreachRecipients.AsNoTracking().AnyAsync(row => row.Id == recipientId && row.LeadId == leadId && row.Status == "Sent", ct);
        if (!valid) return BadRequest(new { message = "This response link is invalid or expired." });
        var alreadyResponded = await db.LeadOutreachResponses.AsNoTracking().AnyAsync(response => response.RecipientId == recipientId, ct);
        return Ok(new { alreadyResponded, message = alreadyResponded ? "A response has already been recorded for this link." : "Your response can be sent securely." });
    }

    [HttpPost]
    [EnableRateLimiting("lead-unsubscribe")]
    public async Task<IActionResult> Submit(SubmitLeadOutreachResponseRequest request, CancellationToken ct)
    {
        if (!TryReadToken(request.Token, out var recipientId, out var leadId)) return BadRequest(new { message = "This response link is invalid or expired." });
        var recipient = await db.LeadOutreachRecipients.Include(row => row.Campaign).FirstOrDefaultAsync(row => row.Id == recipientId && row.LeadId == leadId && row.Status == "Sent", ct);
        if (recipient is null) return BadRequest(new { message = "This response link is invalid or expired." });
        if (await db.LeadOutreachResponses.AnyAsync(response => response.RecipientId == recipientId, ct)) return Conflict(new { message = "A response has already been recorded for this link." });

        var replyMessage = request.Message.Trim();
        var activityDetails = $"Reply received for campaign {recipient.Campaign.Name}: {replyMessage}";
        var response = new LeadOutreachResponse { RecipientId = recipientId, Message = replyMessage, SubmittedAtUtc = DateTime.UtcNow };
        db.LeadOutreachResponses.Add(response);
        db.LeadActivities.Add(new LeadActivity
        {
            LeadId = leadId,
            Type = "ReplyReceived",
            Details = activityDetails[..Math.Min(activityDetails.Length, 1000)],
            Actor = "recipient"
        });
        try
        {
            await db.SaveChangesAsync(ct);
        }
        catch (DbUpdateException)
        {
            // The unique recipient constraint makes concurrent double submissions idempotent.
            db.ChangeTracker.Clear();
            if (await db.LeadOutreachResponses.AsNoTracking().AnyAsync(item => item.RecipientId == recipientId, ct))
                return Conflict(new { message = "A response has already been recorded for this link." });
            throw;
        }
        return Ok(new { message = "Thank you. Your response has been sent to the Rashe Lifesciences team." });
    }

    private bool TryReadToken(string token, out int recipientId, out int leadId)
    {
        recipientId = 0;
        leadId = 0;
        var secret = configuration["Jwt:Key"];
        return !string.IsNullOrWhiteSpace(secret) && LeadOutreachResponseToken.TryRead(token, secret, DateTime.UtcNow, out recipientId, out leadId);
    }
}

public sealed class SubmitLeadOutreachResponseRequest
{
    [Required, StringLength(512, MinimumLength = 20)] public string Token { get; set; } = string.Empty;
    [Required, StringLength(5000, MinimumLength = 2)] public string Message { get; set; } = string.Empty;
}
