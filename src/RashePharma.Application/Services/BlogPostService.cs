using System.Text;
using RashePharma.Application.DTOs.BlogPosts;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Domain.Entities;

namespace RashePharma.Application.Services;

/// <summary>Centralizes CMS rules: unique slugs, safe draft defaults and explicit publishing.</summary>
public sealed class BlogPostService(IBlogPostRepository repository) : IBlogPostService
{
    private const int MaxPageSize = 100;

    public async Task<PagedPublicBlogPostsDto> GetPublishedPageAsync(int page, int pageSize, CancellationToken ct)
    {
        (page, pageSize) = NormalizePaging(page, pageSize);
        var items = await repository.GetPublishedAsync((page - 1) * pageSize, pageSize, ct);
        return new(items.Select(ToPublicSummaryDto).ToArray(), page, pageSize, await repository.CountPublishedAsync(ct));
    }

    public async Task<PublicBlogPostDto?> GetPublishedBySlugAsync(string slug, CancellationToken ct)
    {
        var post = await repository.GetPublishedBySlugAsync(slug.Trim(), ct);
        return post is null ? null : ToPublicDto(post);
    }

    public async Task<PagedBlogPostsDto> GetAdminPageAsync(int page, int pageSize, CancellationToken ct)
    {
        (page, pageSize) = NormalizePaging(page, pageSize);
        var items = await repository.GetAdminPageAsync((page - 1) * pageSize, pageSize, ct);
        return new(items.Select(ToSummaryDto).ToArray(), page, pageSize, await repository.CountAllAsync(ct));
    }

    public async Task<BlogPostDto?> GetByIdAsync(int id, CancellationToken ct)
    {
        var post = await repository.GetByIdAsync(id, ct);
        return post is null ? null : ToDto(post);
    }

    public async Task<BlogPostDto> CreateAsync(BlogPostWriteDto dto, CancellationToken ct)
    {
        var slug = await GetUniqueSlugAsync(dto.Slug, dto.Title, null, ct);
        var post = new BlogPost { Status = "Draft", CreatedAtUtc = DateTime.UtcNow };
        Apply(post, dto, slug);
        await repository.AddAsync(post, ct);
        return ToDto(post);
    }

    public async Task<BlogPostDto?> UpdateAsync(int id, BlogPostWriteDto dto, CancellationToken ct)
    {
        var post = await repository.GetByIdAsync(id, ct);
        if (post is null) return null;
        var slug = await GetUniqueSlugAsync(dto.Slug, dto.Title, id, ct);
        Apply(post, dto, slug);
        post.UpdatedAtUtc = DateTime.UtcNow;
        await repository.SaveAsync(post, ct);
        return ToDto(post);
    }

    public async Task<BlogPostDto?> PublishAsync(int id, CancellationToken ct)
    {
        var post = await repository.GetByIdAsync(id, ct);
        if (post is null) return null;
        // Formatting markers by themselves are not an article; require visible text before publishing.
        if (string.IsNullOrWhiteSpace(post.Title) || !post.Content.Any(char.IsLetterOrDigit))
            throw new InvalidOperationException("A title and article content are required before publishing.");
        post.Status = "Published";
        post.PublishedAtUtc ??= DateTime.UtcNow;
        post.UpdatedAtUtc = DateTime.UtcNow;
        await repository.SaveAsync(post, ct);
        return ToDto(post);
    }

    public async Task<BlogPostDto?> UnpublishAsync(int id, CancellationToken ct)
    {
        var post = await repository.GetByIdAsync(id, ct);
        if (post is null) return null;
        post.Status = "Draft";
        post.PublishedAtUtc = null;
        post.UpdatedAtUtc = DateTime.UtcNow;
        await repository.SaveAsync(post, ct);
        return ToDto(post);
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken ct)
    {
        var post = await repository.GetByIdAsync(id, ct);
        if (post is null) return false;
        await repository.DeleteAsync(post, ct);
        return true;
    }

    private async Task<string> GetUniqueSlugAsync(string? requested, string title, int? id, CancellationToken ct)
    {
        var slug = Slugify(string.IsNullOrWhiteSpace(requested) ? title : requested);
        if (slug.Length is 0 or > 200) throw new InvalidOperationException("Slug must contain 1 to 200 URL-safe characters.");
        if (await repository.SlugExistsAsync(slug, id, ct)) throw new InvalidOperationException("That slug is already used by another blog post.");
        return slug;
    }

    private static string Slugify(string value)
    {
        var builder = new StringBuilder(value.Length);
        var dashPending = false;
        foreach (var character in value.Trim().ToLowerInvariant())
        {
            if (char.IsAsciiLetterOrDigit(character))
            {
                if (dashPending && builder.Length > 0) builder.Append('-');
                builder.Append(character);
                dashPending = false;
            }
            else dashPending = true;
        }
        return builder.ToString().Trim('-');
    }

    private static (int Page, int PageSize) NormalizePaging(int page, int size) => (Math.Max(page, 1), Math.Clamp(size, 1, MaxPageSize));
    private static string? EmptyToNull(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void Apply(BlogPost post, BlogPostWriteDto dto, string slug)
    {
        post.Title = dto.Title.Trim(); post.Slug = slug; post.Excerpt = dto.Excerpt.Trim(); post.Content = dto.Content.Trim();
        post.CoverImageUrl = EmptyToNull(dto.CoverImageUrl); post.AuthorName = dto.AuthorName.Trim();
        post.ReviewerName = EmptyToNull(dto.ReviewerName); post.ReviewedAtUtc = dto.ReviewedAtUtc;
        post.MetaTitle = EmptyToNull(dto.MetaTitle); post.MetaDescription = EmptyToNull(dto.MetaDescription);
        post.CanonicalUrl = EmptyToNull(dto.CanonicalUrl); post.OgTitle = EmptyToNull(dto.OgTitle);
        post.OgDescription = EmptyToNull(dto.OgDescription); post.OgImageUrl = EmptyToNull(dto.OgImageUrl);
        post.IsIndexable = dto.IsIndexable;
    }

    private static BlogPostDto ToDto(BlogPost p) => new()
    {
        Id = p.Id, Title = p.Title, Slug = p.Slug, Excerpt = p.Excerpt, Content = p.Content,
        CoverImageUrl = p.CoverImageUrl, AuthorName = p.AuthorName, ReviewerName = p.ReviewerName,
        ReviewedAtUtc = p.ReviewedAtUtc, Status = p.Status, PublishedAtUtc = p.PublishedAtUtc,
        MetaTitle = p.MetaTitle, MetaDescription = p.MetaDescription, CanonicalUrl = p.CanonicalUrl,
        OgTitle = p.OgTitle, OgDescription = p.OgDescription, OgImageUrl = p.OgImageUrl,
        IsIndexable = p.IsIndexable, CreatedAtUtc = p.CreatedAtUtc, UpdatedAtUtc = p.UpdatedAtUtc
    };

    private static BlogPostSummaryDto ToSummaryDto(BlogPost p) => new()
    {
        Id = p.Id, Title = p.Title, Slug = p.Slug, Status = p.Status,
        PublishedAtUtc = p.PublishedAtUtc, UpdatedAtUtc = p.UpdatedAtUtc
    };

    private static PublicBlogPostSummaryDto ToPublicSummaryDto(BlogPost p) => new()
    {
        Title = p.Title, Slug = p.Slug, Excerpt = p.Excerpt,
        // Resolve private storage behind the API just as the detail view does.
        CoverImageUrl = string.IsNullOrWhiteSpace(p.CoverImageUrl) ? null : $"/api/blog/posts/{Uri.EscapeDataString(p.Slug)}/image",
        AuthorName = p.AuthorName, PublishedAtUtc = p.PublishedAtUtc,
        UpdatedAtUtc = p.UpdatedAtUtc, IsIndexable = p.IsIndexable
    };

    private static PublicBlogPostDto ToPublicDto(BlogPost p) => new()
    {
        Title = p.Title, Slug = p.Slug, Excerpt = p.Excerpt, Content = p.Content,
        // Public clients use the API proxy; the private Azure blob URL stays internal.
        CoverImageUrl = string.IsNullOrWhiteSpace(p.CoverImageUrl) ? null : $"/api/blog/posts/{Uri.EscapeDataString(p.Slug)}/image",
        AuthorName = p.AuthorName, ReviewerName = p.ReviewerName,
        ReviewedAtUtc = p.ReviewedAtUtc, PublishedAtUtc = p.PublishedAtUtc, UpdatedAtUtc = p.UpdatedAtUtc,
        MetaTitle = p.MetaTitle, MetaDescription = p.MetaDescription, CanonicalUrl = p.CanonicalUrl,
        OgTitle = p.OgTitle, OgDescription = p.OgDescription, OgImageUrl = p.OgImageUrl,
        IsIndexable = p.IsIndexable
    };
}
