using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Domain.Entities;

namespace RashePharma.Application.Services;

public class WebsiteContentService : IWebsiteContentService
{
    private readonly IWebsiteContentRepository _repository;

    public WebsiteContentService(
        IWebsiteContentRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<WebsiteContent>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<List<WebsiteContent>> GetBySectionAsync(
        string section)
    {
        return await _repository.GetBySectionAsync(section);
    }

    public async Task<WebsiteContent?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<WebsiteContent?> GetByKeyAsync(
        string section,
        string key)
    {
        return await _repository.GetByKeyAsync(
            section,
            key);
    }

    public async Task<WebsiteContent> CreateAsync(
        WebsiteContent content)
    {
        await _repository.AddAsync(content);

        return content;
    }

    public async Task<WebsiteContent?> UpdateAsync(
        int id,
        WebsiteContent content)
    {
        var existingContent =
            await _repository.GetByIdAsync(id);

        if (existingContent == null)
        {
            return null;
        }

        existingContent.Section = content.Section;
        existingContent.Key = content.Key;
        existingContent.Value = content.Value;
        existingContent.DisplayOrder = content.DisplayOrder;
        existingContent.IsActive = content.IsActive;
        existingContent.UpdatedAt = DateTime.UtcNow;

        await _repository.UpdateAsync(existingContent);

        return existingContent;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var existingContent =
            await _repository.GetByIdAsync(id);

        if (existingContent == null)
        {
            return false;
        }

        await _repository.DeleteAsync(existingContent);

        return true;
    }
}