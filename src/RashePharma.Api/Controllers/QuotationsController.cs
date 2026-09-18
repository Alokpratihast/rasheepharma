using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RashePharma.Application.DTOs.Quotations;
using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class QuotationsController : ControllerBase
{
    private readonly IQuotationService _quotationService;

    public QuotationsController(IQuotationService quotationService)
    {
        _quotationService = quotationService;
    }

    [HttpGet]
    public async Task<ActionResult<List<QuotationListDto>>> GetAll()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        if (User.IsInRole("Admin"))
        {
            var quotations =
                await _quotationService.GetAllAsync();

            return Ok(quotations);
        }

        var userQuotations =
            await _quotationService.GetByUserIdAsync(
                userId.Value);

        return Ok(userQuotations);
    }

    [HttpGet("user/{userId:int}")]
    public async Task<ActionResult<List<QuotationListDto>>> GetByUserId(
        int userId)
    {
        if (!User.IsInRole("Admin"))
            return Forbid();

        var quotations =
            await _quotationService.GetByUserIdAsync(userId);

        return Ok(quotations);
    }

    [HttpGet("enquiry/{enquiryId:int}")]
    public async Task<ActionResult<List<QuotationListDto>>> GetByEnquiryId(
        int enquiryId)
    {
        var currentUserId = GetCurrentUserId();

        if (currentUserId == null)
            return Unauthorized();

        if (User.IsInRole("Admin"))
        {
            var adminQuotations =
                await _quotationService
                    .GetByEnquiryIdForAdminAsync(enquiryId);

            return Ok(adminQuotations);
        }

        var quotations =
            await _quotationService.GetByEnquiryIdAsync(
                enquiryId,
                currentUserId.Value);

        return Ok(quotations);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<QuotationDetailsDto>> GetById(
        int id)
    {
        var currentUserId = GetCurrentUserId();

        if (currentUserId == null)
            return Unauthorized();

        QuotationDetailsDto? quotation;

        if (User.IsInRole("Admin"))
        {
            quotation =
                await _quotationService
                    .GetByIdForAdminAsync(id);
        }
        else
        {
            quotation =
                await _quotationService.GetByIdAsync(
                    id,
                    currentUserId.Value);
        }

        if (quotation == null)
            return NotFound();

        return Ok(quotation);
    }

    [HttpGet("number/{quoteNumber}")]
    public async Task<ActionResult<QuotationDetailsDto>> GetByQuoteNumber(
        string quoteNumber)
    {
        var currentUserId = GetCurrentUserId();

        if (currentUserId == null)
            return Unauthorized();

        QuotationDetailsDto? quotation;

        if (User.IsInRole("Admin"))
        {
            quotation =
                await _quotationService
                    .GetByQuoteNumberForAdminAsync(
                        quoteNumber);
        }
        else
        {
            quotation =
                await _quotationService
                    .GetByQuoteNumberAsync(
                        quoteNumber,
                        currentUserId.Value);
        }

        if (quotation == null)
            return NotFound();

        return Ok(quotation);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<QuotationDetailsDto>> Create(
        CreateQuotationDto dto)
    {
        var currentUserId = GetCurrentUserId();

        if (currentUserId == null)
            return Unauthorized();

        try
        {
            var quotation =
                await _quotationService.CreateAsync(
                    dto,
                    currentUserId.Value);

            return Ok(quotation);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                message = ex.Message
            });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        UpdateQuotationStatusDto dto)
    {
        var updated =
            await _quotationService.UpdateStatusAsync(
                id,
                dto);

        if (!updated)
            return NotFound();

        return NoContent();
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim =
            User.FindFirst(ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
            return null;

        if (!int.TryParse(
                userIdClaim.Value,
                out var userId))
            return null;

        return userId;
    }
}