using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RashePharma.Application.DTOs.Enquiries;
using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnquiriesController : ControllerBase
{
    private readonly IEnquiryService _enquiryService;

    public EnquiriesController(IEnquiryService enquiryService)
    {
        _enquiryService = enquiryService;
    }

    [HttpGet]
    public async Task<ActionResult<List<EnquiryListDto>>> GetAll()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        if (User.IsInRole("Admin"))
        {
            var enquiries = await _enquiryService.GetAllAsync();
            return Ok(enquiries);
        }

        var userEnquiries =
            await _enquiryService.GetByUserIdAsync(userId.Value);

        return Ok(userEnquiries);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<EnquiryDetailsDto>> GetById(int id)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        EnquiryDetailsDto? enquiry;

        if (User.IsInRole("Admin"))
        {
            enquiry =
                await _enquiryService.GetByIdForAdminAsync(id);
        }
        else
        {
            enquiry =
                await _enquiryService.GetByIdAsync(
                    id,
                    userId.Value);
        }

        if (enquiry == null)
            return NotFound();

        return Ok(enquiry);
    }

    [HttpGet("number/{enquiryNumber}")]
    public async Task<ActionResult<EnquiryDetailsDto>> GetByEnquiryNumber(
        string enquiryNumber)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        EnquiryDetailsDto? enquiry;

        if (User.IsInRole("Admin"))
        {
            enquiry =
                await _enquiryService
                    .GetByEnquiryNumberForAdminAsync(enquiryNumber);
        }
        else
        {
            enquiry =
                await _enquiryService
                    .GetByEnquiryNumberAsync(
                        enquiryNumber,
                        userId.Value);
        }

        if (enquiry == null)
            return NotFound();

        return Ok(enquiry);
    }

    [HttpPost]
    public async Task<ActionResult<EnquiryDetailsDto>> Create(
        CreateEnquiryDto dto)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var enquiry =
            await _enquiryService.CreateAsync(
                dto,
                userId.Value);

        return Ok(enquiry);
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        UpdateEnquiryStatusDto dto)
    {
        var updated =
            await _enquiryService.UpdateStatusAsync(
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
