
using RashePharma.Domain.Entities;

namespace RashePharma.Application.Interfaces.Services;

public interface IWebsiteContentService
{
    Task<List<WebsiteContent>> GetAllAsync();

    Task<List<WebsiteContent>> GetBySectionAsync(
        string section);

    Task<WebsiteContent?> GetByIdAsync(int id);

    Task<WebsiteContent?> GetByKeyAsync(
        string section,
        string key);

    Task<WebsiteContent> CreateAsync(
        WebsiteContent content);

    Task<WebsiteContent?> UpdateAsync(
        int id,
        WebsiteContent content);

    Task<bool> DeleteAsync(int id);
}