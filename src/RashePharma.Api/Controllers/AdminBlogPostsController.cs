using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RashePharma.Application.DTOs.BlogPosts;
using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Api.Controllers;

/// <summary>Separate Admin-only endpoints keep CMS writes out of public content APIs.</summary>
[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/blog/posts")]
public sealed class AdminBlogPostsController(IBlogPostService service, IImageStorageService imageStorage) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPage([FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
        => Ok(await service.GetAdminPageAsync(page, pageSize, cancellationToken));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var post = await service.GetByIdAsync(id, cancellationToken);
        return post is null ? NotFound() : Ok(post);
    }

    [HttpPost]
    public async Task<IActionResult> Create(BlogPostWriteDto dto, CancellationToken cancellationToken)
    {
        try { return Ok(await service.CreateAsync(dto, cancellationToken)); }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, BlogPostWriteDto dto, CancellationToken cancellationToken)
    {
        try
        {
            var post = await service.UpdateAsync(id, dto, cancellationToken);
            return post is null ? NotFound() : Ok(post);
        }
        catch (InvalidOperationException ex) { return Conflict(new { message = ex.Message }); }
    }

    [HttpPost("{id:int}/publish")]
    public async Task<IActionResult> Publish(int id, CancellationToken cancellationToken)
    {
        try
        {
            var post = await service.PublishAsync(id, cancellationToken);
            return post is null ? NotFound() : Ok(post);
        }
        catch (InvalidOperationException ex) { return BadRequest(new { message = ex.Message }); }
    }

    [HttpPost("{id:int}/unpublish")]
    public async Task<IActionResult> Unpublish(int id, CancellationToken cancellationToken)
    {
        var post = await service.UnpublishAsync(id, cancellationToken);
        return post is null ? NotFound() : Ok(post);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
        => await service.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();

    [HttpPost("images")]
    [RequestSizeLimit(8 * 1024 * 1024)]
    public async Task<IActionResult> UploadImage(IFormFile file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0 || file.Length > 8 * 1024 * 1024)
            return BadRequest(new { message = "Select an image smaller than 8 MB." });

        var allowedTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
            return BadRequest(new { message = "Only JPEG, PNG, and WebP images are supported." });

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowedExtensions.Contains(extension, StringComparer.Ordinal))
            return BadRequest(new { message = "The image filename extension is not supported." });

        // Validate the file signature too; browser-supplied MIME and filename values are not trusted.
        await using var stream = file.OpenReadStream();
        var signature = new byte[12];
        var bytesRead = await stream.ReadAsync(signature.AsMemory(0, signature.Length), cancellationToken);
        var isJpeg = bytesRead >= 3 && signature[0] == 0xFF && signature[1] == 0xD8 && signature[2] == 0xFF;
        var isPng = bytesRead >= 8 && signature.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
        var isWebp = bytesRead >= 12 && signature.AsSpan(0, 4).SequenceEqual("RIFF"u8) && signature.AsSpan(8, 4).SequenceEqual("WEBP"u8);
        if (!isJpeg && !isPng && !isWebp)
            return BadRequest(new { message = "The uploaded file is not a supported image." });

        var detectedType = isJpeg ? "image/jpeg" : isPng ? "image/png" : "image/webp";
        var expectedExtensions = detectedType switch
        {
            "image/jpeg" => new[] { ".jpg", ".jpeg" },
            "image/png" => new[] { ".png" },
            _ => new[] { ".webp" }
        };
        if (!string.Equals(file.ContentType, detectedType, StringComparison.OrdinalIgnoreCase) ||
            !expectedExtensions.Contains(extension, StringComparer.Ordinal))
            return BadRequest(new { message = "The image content, MIME type, and filename extension must match." });
        stream.Position = 0;

        var url = await imageStorage.UploadAsync(stream, $"blog-{Guid.NewGuid():N}{extension}", file.ContentType);
        return Ok(new { url });
    }
}
