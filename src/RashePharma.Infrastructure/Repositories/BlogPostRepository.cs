using Microsoft.EntityFrameworkCore;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;

namespace RashePharma.Infrastructure.Repositories;

/// <summary>Blog queries are isolated here so public reads always filter at the database.</summary>
public sealed class BlogPostRepository(ApplicationDbContext context) : IBlogPostRepository
{
    public async Task<IReadOnlyList<BlogPost>> GetPublishedAsync(
        int skip, int take, string? search, string sort, CancellationToken ct)
    {
        var query = PublishedQuery(search);
        query = sort switch
        {
            "oldest" => query.OrderBy(p => p.PublishedAtUtc).ThenBy(p => p.Id),
            "title" => query.OrderBy(p => p.Title).ThenBy(p => p.Id),
            _ => query.OrderByDescending(p => p.PublishedAtUtc).ThenByDescending(p => p.Id)
        };

        return await query.Skip(skip).Take(take).ToListAsync(ct);
    }

    public Task<int> CountPublishedAsync(string? search, CancellationToken ct)
        => PublishedQuery(search).CountAsync(ct);

    private IQueryable<BlogPost> PublishedQuery(string? search)
    {
        var query = context.BlogPosts.AsNoTracking().Where(p => p.Status == "Published");
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p =>
                p.Title.Contains(search) ||
                p.Slug.Contains(search) ||
                p.Excerpt.Contains(search) ||
                p.AuthorName.Contains(search));
        }

        return query;
    }

    public Task<BlogPost?> GetPublishedBySlugAsync(string slug, CancellationToken ct)
        => context.BlogPosts.AsNoTracking().FirstOrDefaultAsync(p => p.Slug == slug && p.Status == "Published", ct);

    public async Task<IReadOnlyList<BlogPost>> GetAdminPageAsync(int skip, int take, CancellationToken ct)
        => await context.BlogPosts.AsNoTracking().OrderByDescending(p => p.UpdatedAtUtc)
            .Skip(skip).Take(take).ToListAsync(ct);

    public Task<int> CountAllAsync(CancellationToken ct) => context.BlogPosts.CountAsync(ct);

    public Task<BlogPost?> GetByIdAsync(int id, CancellationToken ct)
        => context.BlogPosts.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<bool> SlugExistsAsync(string slug, int? excludingId, CancellationToken ct)
        => context.BlogPosts.AnyAsync(p => p.Slug == slug && (!excludingId.HasValue || p.Id != excludingId.Value), ct);

    public async Task AddAsync(BlogPost post, CancellationToken ct)
    {
        await context.BlogPosts.AddAsync(post, ct);
        await context.SaveChangesAsync(ct);
    }

    public async Task SaveAsync(BlogPost post, CancellationToken ct)
    {
        await context.SaveChangesAsync(ct);
    }

    public async Task DeleteAsync(BlogPost post, CancellationToken ct)
    {
        context.BlogPosts.Remove(post);
        await context.SaveChangesAsync(ct);
    }
}
