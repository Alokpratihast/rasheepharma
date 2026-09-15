using Microsoft.AspNetCore.Mvc;
using RashePharma.Application.DTOs.Products;
using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductImagesController : ControllerBase
{
    private readonly IProductImageService _imageService;

    public ProductImagesController(
        IProductImageService imageService)
    {
        _imageService = imageService;
    }

    [HttpGet("product/{productId:int}")]
    public async Task<ActionResult<List<ProductImageDto>>> GetByProductId(
        int productId)
    {
        var images = await _imageService
            .GetByProductIdAsync(productId);

        return Ok(images);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProductImageDto>> GetById(int id)
    {
        var image = await _imageService.GetByIdAsync(id);

        if (image == null)
            return NotFound();

        return Ok(image);
    }

    [HttpPost("product/{productId:int}")]
    public async Task<ActionResult<ProductImageDto>> Create(
        int productId,
        ProductImageCreateDto dto)
    {
        try
        {
            var image = await _imageService
                .CreateAsync(productId, dto);

            return CreatedAtAction(
                nameof(GetById),
                new { id = image.Id },
                image);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProductImageDto>> Update(
        int id,
        ProductImageUpdateDto dto)
    {
        var image = await _imageService.UpdateAsync(id, dto);

        if (image == null)
            return NotFound();

        return Ok(image);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _imageService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}