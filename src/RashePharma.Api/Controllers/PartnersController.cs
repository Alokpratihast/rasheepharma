using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RashePharma.Application.DTOs.Partners;
using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PartnersController : ControllerBase
{
    private readonly IPartnerService _partnerService;

    public PartnersController(IPartnerService partnerService)
    {
        _partnerService = partnerService;
    }

    // =========================
    // ADMIN APIs
    // =========================

    [Authorize(Roles = "Admin")]
    [HttpGet("requests")]
    public async Task<ActionResult<List<PartnerRequestListDto>>> GetAllRequests()
    {
        var requests = await _partnerService.GetAllRequestsAsync();

        return Ok(requests);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("requests/{id:int}")]
    public async Task<ActionResult<PartnerRequestDetailsDto>> GetRequestById(
        int id)
    {
        var request = await _partnerService.GetRequestByIdAsync(id);

        if (request == null)
            return NotFound();

        return Ok(request);
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("requests/{id:int}/status")]
    public async Task<IActionResult> UpdateRequestStatus(
        int id,
        UpdatePartnerRequestStatusDto dto)
    {
        var updated = await _partnerService
            .UpdateRequestStatusAsync(id, dto);

        if (!updated)
            return NotFound();

        return NoContent();
    }

    // =========================
    // AUTHENTICATED USER API
    // =========================

    [Authorize]
    [HttpGet("requests/user/{userId:int}")]
    public async Task<ActionResult<List<PartnerRequestListDto>>> GetMyRequests(
        int userId)
    {
        var currentUserId = GetCurrentUserId();

        if (currentUserId == null)
            return Unauthorized();

        if (currentUserId.Value != userId)
            return Forbid();

        var requests = await _partnerService.GetMyRequestsAsync(userId);

        return Ok(requests);
    }

    [Authorize]
    [HttpPost("requests")]
    public async Task<ActionResult<PartnerRequestDetailsDto>> CreateRequest(
        PartnerRequestCreateDto dto)
    {
        var currentUserId = GetCurrentUserId();

        if (currentUserId == null)
            return Unauthorized();

        var request = await _partnerService
            .CreateRequestAsync(dto, currentUserId.Value);

        return Ok(request);
    }

    // =========================
    // PUBLIC APIs
    // =========================

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<PartnerDto>>> GetAllPartners()
    {
        var partners = await _partnerService.GetAllPartnersAsync();

        return Ok(partners);
    }

    [AllowAnonymous]
    [HttpGet("{id:int}")]
    public async Task<ActionResult<PartnerDto>> GetPartnerById(int id)
    {
        var partner = await _partnerService.GetPartnerByIdAsync(id);

        if (partner == null)
            return NotFound();

        return Ok(partner);
    }

    // =========================
    // AUTHENTICATED USER API
    // =========================

    [Authorize]
    [HttpGet("user/{userId:int}")]
    public async Task<ActionResult<PartnerDto>> GetMyPartner(
        int userId)
    {
        var currentUserId = GetCurrentUserId();

        if (currentUserId == null)
            return Unauthorized();

        if (currentUserId.Value != userId)
            return Forbid();

        var partner = await _partnerService.GetMyPartnerAsync(userId);

        if (partner == null)
            return NotFound();

        return Ok(partner);
    }

    // =========================
    // HELPER
    // =========================

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrEmpty(userIdClaim))
            return null;

        return int.TryParse(userIdClaim, out var userId)
            ? userId
            : null;
    }
}