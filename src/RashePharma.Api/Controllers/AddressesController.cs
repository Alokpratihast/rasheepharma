using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RashePharma.Application.DTOs.Addresses;
using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AddressesController : ControllerBase
{
    private readonly IAddressService _addressService;

    public AddressesController(IAddressService addressService)
    {
        _addressService = addressService;
    }

    [HttpGet]
    public async Task<ActionResult<List<AddressListDto>>> GetMyAddresses()
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var addresses = await _addressService.GetByUserIdAsync(
            userId.Value);

        return Ok(addresses);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<AddressDetailsDto>> GetById(int id)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var address = await _addressService.GetByIdAsync(
            id,
            userId.Value);

        if (address == null)
            return NotFound();

        return Ok(address);
    }

    [HttpPost]
    public async Task<ActionResult<AddressDetailsDto>> Create(
        AddressCreateDto dto)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var address = await _addressService.CreateAsync(
            userId.Value,
            dto);

        return Ok(address);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<AddressDetailsDto>> Update(
        int id,
        AddressUpdateDto dto)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var address = await _addressService.UpdateAsync(
            id,
            userId.Value,
            dto);

        if (address == null)
            return NotFound();

        return Ok(address);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();

        if (userId == null)
            return Unauthorized();

        var deleted = await _addressService.DeleteAsync(
            id,
            userId.Value);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    private int? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(
            ClaimTypes.NameIdentifier);

        if (userIdClaim == null)
            return null;

        if (!int.TryParse(
                userIdClaim.Value,
                out var userId))
            return null;

        return userId;
    }
}