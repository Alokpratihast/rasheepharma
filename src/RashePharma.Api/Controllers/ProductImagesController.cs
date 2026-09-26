using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RashePharma.Application.DTOs.Products;
using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductImagesController : ControllerBase
{
    private readonly IProductImageService _imageService;
    private readonly IImageStorageService _imageStorageService;

    public ProductImagesController(
        IProductImageService imageService,
        IImageStorageService imageStorageService)
    {
        _imageService = imageService;
        _imageStorageService = imageStorageService;
    }

    // =========================
    // PUBLIC APIs
    // =========================

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

    // =========================
    // IMAGE FILE
    // =========================

    [HttpGet("file/{id:int}")]
    public async Task<IActionResult> GetImageFile(int id)
    {
        var image = await _imageService.GetByIdAsync(id);

        if (image == null)
            return NotFound();

        try
        {
            var result =
                await _imageStorageService.DownloadAsync(
                    image.ImageUrl);

            return File(
                result.Stream,
                result.ContentType);
        }
        catch (Azure.RequestFailedException ex)
            when (ex.Status == 404)
        {
            return NotFound(new
            {
                message = "Image file was not found in Azure Blob Storage."
            });
        }
    }

    // =========================
    // ADMIN APIs
    // =========================

    [Authorize(Roles = "Admin")]
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

    [Authorize(Roles = "Admin")]
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

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _imageService.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [Authorize(Roles = "Admin")]
    [HttpPost("product/{productId:int}/upload")]
    public async Task<ActionResult<ProductImageDto>> Upload(
        int productId,
        IFormFile file,
        [FromForm] string? altText,
        [FromForm] bool isPrimary = false,
        [FromForm] int displayOrder = 0)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new
                {
                    message = "Image file is required."
                });
            }

            var image = await _imageService.UploadAsync(
                productId,
                file.OpenReadStream(),
                file.FileName,
                file.ContentType,
                altText,
                isPrimary,
                displayOrder);

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
}