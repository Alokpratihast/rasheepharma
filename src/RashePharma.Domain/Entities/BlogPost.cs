namespace RashePharma.Domain.Entities;

/// <summary>
/// A standalone article managed by the blog CMS. Keeping blog content separate
/// from WebsiteContent prevents article lifecycle and SEO fields from changing
/// the existing page-content contract.
/// </summary>
public class BlogPost
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
    public bool IsIndexable { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
