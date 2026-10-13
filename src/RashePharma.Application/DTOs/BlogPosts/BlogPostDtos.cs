using System.ComponentModel.DataAnnotations;

namespace RashePharma.Application.DTOs.BlogPosts;

public sealed class BlogPostWriteDto
{
    [Required, StringLength(180, MinimumLength = 3)]
    public string Title { get; set; } = string.Empty;
    [StringLength(200)]
    public string? Slug { get; set; }
    [Required, StringLength(500)]
    public string Excerpt { get; set; } = string.Empty;
    [Required, StringLength(100000)]
    public string Content { get; set; } = string.Empty;
    [StringLength(2048)]
    public string? CoverImageUrl { get; set; }
    [Required, StringLength(120)]
    public string AuthorName { get; set; } = string.Empty;
    [StringLength(120)]
    public string? ReviewerName { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
    [StringLength(180)]
    public string? MetaTitle { get; set; }
    [StringLength(320)]
    public string? MetaDescription { get; set; }
    [Url, StringLength(2048)]
    public string? CanonicalUrl { get; set; }
    [StringLength(180)]
    public string? OgTitle { get; set; }
    [StringLength(320)]
    public string? OgDescription { get; set; }
    [StringLength(2048)]
    public string? OgImageUrl { get; set; }
    public bool IsIndexable { get; set; } = true;
}

public sealed class BlogPostDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? ReviewerName { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
    public string Status { get; set; } = "Draft";
    public DateTime? PublishedAtUtc { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? CanonicalUrl { get; set; }
    public string? OgTitle { get; set; }
    public string? OgDescription { get; set; }
    public string? OgImageUrl { get; set; }
    public bool IsIndexable { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

/// <summary>List rows omit the large article body to keep admin pages responsive.</summary>
public sealed class BlogPostSummaryDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";
    public DateTime? PublishedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

/// <summary>Public response omits admin-only workflow and audit properties.</summary>
public sealed class PublicBlogPostDto
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string? ReviewerName { get; set; }
    public DateTime? ReviewedAtUtc { get; set; }
    public DateTime? PublishedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public string? MetaTitle { get; set; }
    public string? MetaDescription { get; set; }
    public string? CanonicalUrl { get; set; }
    public string? OgTitle { get; set; }
    public string? OgDescription { get; set; }
    public string? OgImageUrl { get; set; }
    public bool IsIndexable { get; set; }
}

/// <summary>Public listing cards need excerpts, not up to 100 KB of article body per row.</summary>
public sealed class PublicBlogPostSummaryDto
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Excerpt { get; set; } = string.Empty;
    public string? CoverImageUrl { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public DateTime? PublishedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
    public bool IsIndexable { get; set; }
}

public sealed record PagedBlogPostsDto(IReadOnlyList<BlogPostSummaryDto> Items, int Page, int PageSize, int TotalCount);
public sealed record PagedPublicBlogPostsDto(IReadOnlyList<PublicBlogPostSummaryDto> Items, int Page, int PageSize, int TotalCount);
