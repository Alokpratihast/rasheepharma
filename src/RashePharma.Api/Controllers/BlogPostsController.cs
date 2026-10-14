using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Api.Controllers;

/// <summary>Public routes return published content only; draft access stays in the admin controller.</summary>
[ApiController]
[Route("api/blog/posts")]
public sealed class BlogPostsController(
    IBlogPostService service,
    IImageStorageService imageStorage,
    ILogger<BlogPostsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetPublished(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] string? q = null,
        [FromQuery] string? sort = "newest",
        CancellationToken cancellationToken = default)
        => Ok(await service.GetPublishedPageAsync(page, pageSize, q, sort, cancellationToken));

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(
        string slug,
        CancellationToken cancellationToken)
    {
        var post = await service.GetPublishedBySlugAsync(slug, cancellationToken);

        if (post is null)
        {
            logger.LogWarning(
                "Published blog post not found for slug {Slug}", slug);
            return NotFound();
        }

        return Ok(post);
    }

    // The Azure container is private. Stream approved blog covers through the API instead of making storage public.
    [HttpGet("{slug}/image")]
    public async Task<IActionResult> GetCoverImage(
        string slug,
        CancellationToken cancellationToken)
    {
        // Public post DTOs intentionally replace the private Blob URL with this proxy route.
        // Resolve the persisted Blob URL through the dedicated service method for the download.
        var coverImageUrl = await service.GetPublishedCoverImageUrlAsync(
            slug,
            cancellationToken);

        if (string.IsNullOrWhiteSpace(coverImageUrl))
        {
            logger.LogWarning(
                "Blog cover request: published post or cover image not found for slug {Slug}", slug);
            return NotFound();
        }

        try
        {
            var image = await imageStorage.DownloadAsync(coverImageUrl);

            // Blob names are immutable/unique; cache the proxy response and prevent MIME sniffing.
            Response.Headers.CacheControl =
                "public, max-age=86400, stale-while-revalidate=604800";
            Response.Headers.XContentTypeOptions = "nosniff";

            logger.LogDebug(
                "Blog cover downloaded successfully for slug {Slug}", slug);

            return File(image.Stream, image.ContentType);
        }
        catch (Azure.RequestFailedException ex) when (ex.Status == 404)
        {
            logger.LogError(
                ex,
                "Azure Blob returned 404 while downloading cover for blog slug {Slug}",
                slug);
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            // Invalid legacy/admin-supplied image URLs should fail closed as unavailable.
            logger.LogError(
                ex,
                "Blog cover URL validation failed for slug {Slug}", slug);
            return NotFound();
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Unexpected error while serving blog cover for slug {Slug}", slug);
            throw;
        }
    }
}
