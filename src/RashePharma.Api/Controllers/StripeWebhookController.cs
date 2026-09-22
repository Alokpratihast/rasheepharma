using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RashePharma.Application.Interfaces.Services;
using Stripe;

namespace RashePharma.Api.Controllers;

[ApiController]
[Route("api/stripe/webhook")]
public class StripeWebhookController : ControllerBase
{
    private readonly IConfiguration _configuration;
    private readonly IPaymentService _paymentService;

    public StripeWebhookController(
        IConfiguration configuration,
        IPaymentService paymentService)
    {
        _configuration = configuration;
        _paymentService = paymentService;
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult> HandleWebhook()
    {
        Console.WriteLine(
            "========== STRIPE WEBHOOK HIT ==========");

        var json = await new StreamReader(
            HttpContext.Request.Body).ReadToEndAsync();

        var webhookSecret =
            Environment.GetEnvironmentVariable(
                "Stripe__WebhookSecret");

        Console.WriteLine(
            $"Webhook secret configured: {!string.IsNullOrWhiteSpace(webhookSecret)}");

        if (string.IsNullOrWhiteSpace(webhookSecret))
        {
            return BadRequest(
                "Stripe webhook secret is not configured.");
        }

        var stripeSignature =
            Request.Headers["Stripe-Signature"].ToString();

        if (string.IsNullOrWhiteSpace(stripeSignature))
        {
            return BadRequest(
                "Stripe signature is missing.");
        }

        Stripe.Event stripeEvent;

        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                json,
                stripeSignature,
                webhookSecret,
                throwOnApiVersionMismatch: false);
        }
        catch (StripeException ex)
        {
            Console.WriteLine(
                $"Stripe webhook error: {ex.Message}");

            return BadRequest(
                $"Invalid Stripe webhook signature: {ex.Message}");
        }

        Console.WriteLine(
            $"Stripe Event Received: {stripeEvent.Type}");

        await _paymentService.HandleWebhookAsync(
            json,
            stripeSignature);

        return Ok();
    }
}