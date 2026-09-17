using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RashePharma.Application.DTOs.Products;
using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductVariantsController : ControllerBase
{
    private readonly IProductVariantService _variantService;

    public ProductVariantsController(
        IProductVariantService variantService)
    {
        _variantService = variantService;
    }

    // =========================
    // PUBLIC APIs
    // =========================

    [HttpGet("product/{productId:int}")]
    public async Task<ActionResult<List<ProductVariantDto>>> GetByProductId(
        int productId)
    {
        var variants = await _variantService
            .GetByProductIdAsync(productId);

        return Ok(variants);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductVariantDto>> GetById(int id)
    {
        var variant = await _variantService.GetByIdAsync(id);

        if (variant == null)
            return NotFound();

        return Ok(variant);
    }

    // =========================
    // ADMIN APIs
    // =========================

    [Authorize(Roles = "Admin")]
    [HttpPost("product/{productId:int}")]
    public async Task<ActionResult<ProductVariantDto>> Create(
        int productId,
        ProductVariantCreateDto dto)
    {
        try
        {
            var variant = await _variantService
                .CreateAsync(productId, dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = variant.Id },
                variant);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductVariantDto>> Update(
        int id,
        ProductVariantUpdateDto dto)
    {
        var variant = await _variantService
            .UpdateAsync(id, dto);

        if (variant == null)
            return NotFound();

        return Ok(variant);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _variantService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}