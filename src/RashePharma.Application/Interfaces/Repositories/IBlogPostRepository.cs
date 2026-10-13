using RashePharma.Domain.Entities;

namespace RashePharma.Application.Interfaces.Repositories;

public interface IBlogPostRepository
{
    Task<IReadOnlyList<BlogPost>> GetPublishedAsync(int skip, int take, CancellationToken cancellationToken);
    Task<int> CountPublishedAsync(CancellationToken cancellationToken);
    Task<BlogPost?> GetPublishedBySlugAsync(string slug, CancellationToken cancellationToken);
    Task<IReadOnlyList<BlogPost>> GetAdminPageAsync(int skip, int take, CancellationToken cancellationToken);
    Task<int> CountAllAsync(CancellationToken cancellationToken);
    Task<BlogPost?> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<bool> SlugExistsAsync(string slug, int? excludingId, CancellationToken cancellationToken);
    Task AddAsync(BlogPost post, CancellationToken cancellationToken);
    Task SaveAsync(BlogPost post, CancellationToken cancellationToken);
    Task DeleteAsync(BlogPost post, CancellationToken cancellationToken);
}
