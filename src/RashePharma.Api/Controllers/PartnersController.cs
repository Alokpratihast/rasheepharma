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

    [HttpGet("requests")]
    public async Task<ActionResult<List<PartnerRequestListDto>>> GetAllRequests()
    {
        var requests = await _partnerService.GetAllRequestsAsync();

        return Ok(requests);
    }

    [HttpGet("requests/{id:int}")]
    public async Task<ActionResult<PartnerRequestDetailsDto>> GetRequestById(
        int id)
    {
        var request = await _partnerService.GetRequestByIdAsync(id);

        if (request == null)
            return NotFound();

        return Ok(request);
    }

    [HttpGet("requests/user/{userId:int}")]
    public async Task<ActionResult<List<PartnerRequestListDto>>> GetMyRequests(
        int userId)
    {
        var requests = await _partnerService.GetMyRequestsAsync(userId);

        return Ok(requests);
    }

    [HttpPost("requests")]
    public async Task<ActionResult<PartnerRequestDetailsDto>> CreateRequest(
        PartnerRequestCreateDto dto)
    {
        var request = await _partnerService.CreateRequestAsync(dto, null);

        return Ok(request);
    }

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

    [HttpGet]
    public async Task<ActionResult<List<PartnerDto>>> GetAllPartners()
    {
        var partners = await _partnerService.GetAllPartnersAsync();

        return Ok(partners);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<PartnerDto>> GetPartnerById(int id)
    {
        var partner = await _partnerService.GetPartnerByIdAsync(id);

        if (partner == null)
            return NotFound();

        return Ok(partner);
    }

    [HttpGet("user/{userId:int}")]
    public async Task<ActionResult<PartnerDto>> GetMyPartner(
        int userId)
    {
        var partner = await _partnerService.GetMyPartnerAsync(userId);

        if (partner == null)
            return NotFound();

        return Ok(partner);
    }
}