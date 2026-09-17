using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Domain.Entities;

namespace RashePharma.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebsiteContentController : ControllerBase
{
    private readonly IWebsiteContentService _service;

    public WebsiteContentController(
        IWebsiteContentService service)
    {
        _service = service;
    }

    // =========================
    // PUBLIC APIs
    // =========================

    [HttpGet]
    public async Task<ActionResult<List<WebsiteContent>>> GetAll()
    {
        var content = await _service.GetAllAsync();

        return Ok(content);
    }

    [HttpGet("section/{section}")]
    public async Task<ActionResult<List<WebsiteContent>>> GetBySection(
        string section)
    {
        var content = await _service.GetBySectionAsync(section);

        return Ok(content);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<WebsiteContent>> GetById(int id)
    {
        var content = await _service.GetByIdAsync(id);

        if (content == null)
        {
            return NotFound();
        }

        return Ok(content);
    }

    [HttpGet("key/{section}/{key}")]
    public async Task<ActionResult<WebsiteContent>> GetByKey(
        string section,
        string key)
    {
        var content = await _service.GetByKeyAsync(
            section,
            key);

        if (content == null)
        {
            return NotFound();
        }

        return Ok(content);
    }

    // =========================
    // ADMIN APIs
    // =========================

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<WebsiteContent>> Create(
        WebsiteContent content)
    {
        var createdContent =
            await _service.CreateAsync(content);

        return CreatedAtAction(
            nameof(GetById),
            new { id = createdContent.Id },
            createdContent);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<WebsiteContent>> Update(
        int id,
        WebsiteContent content)
    {
        var updatedContent =
            await _service.UpdateAsync(id, content);

        if (updatedContent == null)
        {
            return NotFound();
        }

        return Ok(updatedContent);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted =
            await _service.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound();
        }

        return NoContent();
    }
}