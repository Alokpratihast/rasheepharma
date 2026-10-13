using RashePharma.Application.DTOs.BlogPosts;

namespace RashePharma.Application.Interfaces.Services;

public interface IBlogPostService
{
    Task<PagedPublicBlogPostsDto> GetPublishedPageAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<PublicBlogPostDto?> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<PagedBlogPostsDto> GetAdminPageAsync(int page, int pageSize, CancellationToken cancellationToken);
    Task<BlogPostDto?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<BlogPostDto> CreateAsync(BlogPostWriteDto dto, CancellationToken cancellationToken);
    Task<BlogPostDto?> UpdateAsync(int id, BlogPostWriteDto dto, CancellationToken cancellationToken);
    Task<BlogPostDto?> PublishAsync(int id, CancellationToken cancellationToken);
    Task<BlogPostDto?> UnpublishAsync(int id, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken);
}
