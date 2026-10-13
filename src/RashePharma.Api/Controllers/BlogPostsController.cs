using Microsoft.AspNetCore.Mvc;
using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Api.Controllers;

/// <summary>Public routes return published content only; draft access stays in the admin controller.</summary>
[ApiController]
[Route("api/blog/posts")]
public sealed class BlogPostsController(IBlogPostService service, IImageStorageService imageStorage) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPublished([FromQuery] int page = 1, [FromQuery] int pageSize = 12, CancellationToken cancellationToken = default)
        => Ok(await service.GetPublishedPageAsync(page, pageSize, cancellationToken));

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug, CancellationToken cancellationToken)
    {
        var post = await service.GetPublishedBySlugAsync(slug, cancellationToken);
        return post is null ? NotFound() : Ok(post);
    }

    // The Azure container is private. Stream approved blog covers through the API instead of making storage public.
    [HttpGet("{slug}/image")]
    public async Task<IActionResult> GetCoverImage(string slug, CancellationToken cancellationToken)
    {
        var post = await service.GetPublishedBySlugAsync(slug, cancellationToken);
        if (post is null || string.IsNullOrWhiteSpace(post.CoverImageUrl)) return NotFound();
        try
        {
            var image = await imageStorage.DownloadAsync(post.CoverImageUrl);
            // Blob names are immutable/unique; cache the proxy response and prevent MIME sniffing.
            Response.Headers.CacheControl = "public, max-age=86400, stale-while-revalidate=604800";
            Response.Headers.XContentTypeOptions = "nosniff";
            return File(image.Stream, image.ContentType);
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            return NotFound();
        }
        catch (ArgumentException)
        {
            // Invalid legacy/admin-supplied image URLs should fail closed as unavailable.
            return NotFound();
        }
    }
}
