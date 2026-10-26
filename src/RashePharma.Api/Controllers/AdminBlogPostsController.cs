using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RashePharma.Application.DTOs.BlogPosts;
using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Api.Controllers;

/// <summary>Separate Admin-only endpoints keep CMS writes out of public content APIs.</summary>
[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/blog/posts")]
public sealed class AdminBlogPostsController(IBlogPostService service, IBlogImageStorageService imageStorage) : ControllerBase
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

    // Admin previews must stay behind authentication because the storage container is private.
    [HttpGet("{id:int}/image")]
    public async Task<IActionResult> GetCoverImage(int id, [FromQuery] bool original = false, CancellationToken cancellationToken = default)
    {
        var post = await service.GetByIdAsync(id, cancellationToken);
        var coverUrl = original
            ? post?.OriginalCoverImageUrl ?? post?.CoverImageUrl
            : post?.CoverImageUrl;
        if (post is null || string.IsNullOrWhiteSpace(coverUrl))
            return NotFound();

        try
        {
            var image = await imageStorage.DownloadAsync(coverUrl);
            if (image.ContentType is not ("image/jpeg" or "image/png" or "image/webp"))
            {
                await image.Stream.DisposeAsync();
                return NotFound();
            }

            Response.Headers.CacheControl = "private, no-store";
            Response.Headers.XContentTypeOptions = "nosniff";
            return File(image.Stream, image.ContentType);
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            return NotFound();
        }
        catch (ArgumentException)
        {
            return NotFound();
        }
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
    [RequestSizeLimit(17 * 1024 * 1024)]
    public async Task<IActionResult> UploadImage(
        [FromForm] IFormFile file,
        [FromForm] IFormFile displayFile,
        CancellationToken cancellationToken)
    {
        var originalError = await ValidateImageAsync(file, cancellationToken);
        if (originalError is not null)
            return BadRequest(new { message = $"Original image: {originalError}" });

        var displayError = await ValidateImageAsync(displayFile, cancellationToken);
        if (displayError is not null)
            return BadRequest(new { message = $"16:10 image: {displayError}" });
        if (!string.Equals(Path.GetExtension(displayFile.FileName), ".webp", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(displayFile.ContentType, "image/webp", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "The generated 16:10 cover must be a WebP image." });
        if (!await IsExpectedDisplaySizeAsync(displayFile, cancellationToken))
            return BadRequest(new { message = "The display image must be exactly 1200 × 750 pixels (16:10)." });

        await using var originalStream = file.OpenReadStream();
        var originalExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var originalUrl = await imageStorage.UploadAsync(
            originalStream, $"blog-original-{Guid.NewGuid():N}{originalExtension}", file.ContentType);

        await using var displayStream = displayFile.OpenReadStream();
        var displayUrl = await imageStorage.UploadAsync(
            displayStream, $"blog-cover-16x10-{Guid.NewGuid():N}.webp", "image/webp");

        return Ok(new { url = displayUrl, originalUrl });
    }

    private static async Task<string?> ValidateImageAsync(IFormFile? file, CancellationToken cancellationToken)
    {
        if (file is null || file.Length == 0 || file.Length > 8 * 1024 * 1024)
            return "Select an image smaller than 8 MB.";

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowedExtensions.Contains(extension, StringComparer.Ordinal))
            return "The image filename extension is not supported.";

        await using var stream = file.OpenReadStream();
        var signature = new byte[12];
        var bytesRead = await stream.ReadAsync(signature.AsMemory(0, signature.Length), cancellationToken);
        var isJpeg = bytesRead >= 3 && signature[0] == 0xFF && signature[1] == 0xD8 && signature[2] == 0xFF;
        var isPng = bytesRead >= 8 && signature.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A });
        var isWebp = bytesRead >= 12 && signature.AsSpan(0, 4).SequenceEqual("RIFF"u8) && signature.AsSpan(8, 4).SequenceEqual("WEBP"u8);
        if (!isJpeg && !isPng && !isWebp)
            return "The uploaded file is not a supported image.";

        var detectedType = isJpeg ? "image/jpeg" : isPng ? "image/png" : "image/webp";
        var expectedExtensions = detectedType switch
        {
            "image/jpeg" => new[] { ".jpg", ".jpeg" },
            "image/png" => new[] { ".png" },
            _ => new[] { ".webp" }
        };
        if (!string.Equals(file.ContentType, detectedType, StringComparison.OrdinalIgnoreCase) ||
            !expectedExtensions.Contains(extension, StringComparer.Ordinal))
            return "The image content, MIME type, and filename extension must match.";

        return null;
    }

    private static async Task<bool> IsExpectedDisplaySizeAsync(IFormFile file, CancellationToken cancellationToken)
    {
        // Canvas exports VP8 WebP. Parse the header so the API, rather than only the browser UI,
        // enforces the promised display ratio before the derivative is stored.
        var header = new byte[30];
        await using var stream = file.OpenReadStream();
        var bytesRead = 0;
        while (bytesRead < header.Length)
        {
            var read = await stream.ReadAsync(header.AsMemory(bytesRead), cancellationToken);
            if (read == 0) break;
            bytesRead += read;
        }

        if (bytesRead < 30 || !header.AsSpan(0, 4).SequenceEqual("RIFF"u8) ||
            !header.AsSpan(8, 4).SequenceEqual("WEBP"u8))
            return false;

        int width;
        int height;
        if (header.AsSpan(12, 4).SequenceEqual("VP8 "u8) &&
            header[23] == 0x9D && header[24] == 0x01 && header[25] == 0x2A)
        {
            width = (header[26] | (header[27] << 8)) & 0x3FFF;
            height = (header[28] | (header[29] << 8)) & 0x3FFF;
        }
        else if (header.AsSpan(12, 4).SequenceEqual("VP8L"u8) && header[20] == 0x2F)
        {
            var bits = (uint)(header[21] | (header[22] << 8) | (header[23] << 16) | (header[24] << 24));
            width = (int)(bits & 0x3FFF) + 1;
            height = (int)((bits >> 14) & 0x3FFF) + 1;
        }
        else if (header.AsSpan(12, 4).SequenceEqual("VP8X"u8))
        {
            width = header[24] | (header[25] << 8) | (header[26] << 16);
            height = header[27] | (header[28] << 8) | (header[29] << 16);
            width++;
            height++;
        }
        else
        {
            return false;
        }

        return width == 1200 && height == 750;
    }
}
