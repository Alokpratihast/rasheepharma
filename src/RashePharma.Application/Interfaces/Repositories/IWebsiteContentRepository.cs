using RashePharma.Domain.Entities;

namespace RashePharma.Application.Interfaces.Repositories;

public interface IWebsiteContentRepository
{
    Task<List<WebsiteContent>> GetAllAsync();

    Task<List<WebsiteContent>> GetBySectionAsync(
        string section);

    Task<WebsiteContent?> GetByIdAsync(int id);

    Task<WebsiteContent?> GetByKeyAsync(
        string section,
        string key);

    Task AddAsync(WebsiteContent content);

    Task UpdateAsync(WebsiteContent content);

    Task DeleteAsync(WebsiteContent content);
}