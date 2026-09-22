using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RashePharma.Application.DTOs.Payments;
using RashePharma.Application.Interfaces.Services;
using System.Security.Claims;

namespace RashePharma.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(
        IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("create-checkout-session")]
    public async Task<IActionResult> CreateCheckoutSession(
        [FromBody] CreateCheckoutSessionRequest request)
    {
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!int.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized();
        }

        try
        {
            var checkoutUrl =
                await _paymentService.CreateCheckoutSessionAsync(
                    userId,
                    request);

            return Ok(new
            {
                checkoutUrl
            });
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }
}