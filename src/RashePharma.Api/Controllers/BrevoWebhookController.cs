using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RashePharma.Infrastructure.Services;

namespace RashePharma.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/webhooks/brevo")]
public sealed class BrevoWebhookController(
    IConfiguration configuration,
    BrevoWebhookProcessor processor,
    ILogger<BrevoWebhookController> logger) : ControllerBase
{
    private const int MaxPayloadBytes = 256 * 1024;

    [HttpPost("transactional")]
    [EnableRateLimiting("brevo-webhook")]
    [RequestSizeLimit(MaxPayloadBytes)]
    public async Task<IActionResult> Transactional(CancellationToken cancellationToken)
    {
        if (Request.ContentLength is > MaxPayloadBytes)
            return StatusCode(StatusCodes.Status413PayloadTooLarge);

        var configuredToken = configuration["EmailWebhook:BearerToken"];
        var authorization = Request.Headers.Authorization.ToString();
        var suppliedToken = authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
            ? authorization[7..].Trim()
            : string.Empty;
        if (string.IsNullOrWhiteSpace(configuredToken) || configuredToken.Length < 32 ||
            string.IsNullOrWhiteSpace(suppliedToken) ||
            !CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(configuredToken), Encoding.UTF8.GetBytes(suppliedToken)))
            return Unauthorized();

        JsonDocument document;
        try
        {
            document = await JsonDocument.ParseAsync(
                Request.Body,
                new JsonDocumentOptions { MaxDepth = 16 },
                cancellationToken);
        }
        catch (JsonException)
        {
            return BadRequest(new { message = "Webhook payload must be valid JSON." });
        }

        using (document)
        {
            var root = document.RootElement;
            if (root.ValueKind == JsonValueKind.Array)
            {
                if (root.GetArrayLength() > 100)
                    return BadRequest(new { message = "Webhook event batch is too large." });
                foreach (var item in root.EnumerateArray())
                {
                    if (!BrevoWebhookEventParser.TryParse(item, out _))
                        return BadRequest(new { message = "Webhook batch contains an invalid event." });
                    await ProcessSafelyAsync(item, cancellationToken);
                }
            }
            else
            {
                if (!BrevoWebhookEventParser.TryParse(root, out _))
                    return BadRequest(new { message = "Webhook event is invalid." });
                await ProcessSafelyAsync(root, cancellationToken);
            }
        }

        return Accepted();
    }

    private async Task ProcessSafelyAsync(JsonElement payload, CancellationToken cancellationToken)
    {
        var result = await processor.ProcessAsync(payload, cancellationToken);
        if (result == BrevoWebhookProcessResult.Unmatched)
            logger.LogWarning("Ignored a Brevo webhook event that did not match an email job and recipient.");
    }
}
