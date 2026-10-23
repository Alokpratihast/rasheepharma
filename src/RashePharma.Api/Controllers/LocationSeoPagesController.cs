using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;

namespace RashePharma.Api.Controllers;

public sealed class LocationSeoPageWriteDto
{
    [Required, StringLength(100)] public string Country { get; set; } = string.Empty;
    [Required, RegularExpression("^[A-Z]{2}$")] public string CountryCode { get; set; } = string.Empty;
    [Required, RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$")] public string CountrySlug { get; set; } = string.Empty;
    [Required, RegularExpression("^(INR|USD)$")] public string QuoteCurrency { get; set; } = "USD";
    [Required, StringLength(120)] public string City { get; set; } = string.Empty;
    [Required, StringLength(120)] public string Region { get; set; } = string.Empty;
    [Required, RegularExpression("^[a-z0-9]+(?:-[a-z0-9]+)*$")] public string CitySlug { get; set; } = string.Empty;
    [Required, StringLength(200)] public string MetaTitle { get; set; } = string.Empty;
    [Required, StringLength(320)] public string MetaDescription { get; set; } = string.Empty;
    [Required, StringLength(200)] public string Heading { get; set; } = string.Empty;
    [Required, StringLength(3000)] public string Intro { get; set; } = string.Empty;
    [Required, StringLength(2000)] public string BuyerFocus { get; set; } = string.Empty;
    public bool IsPublished { get; set; } = true;
    public bool IsIndexable { get; set; } = true;
}

[ApiController]
[Route("api/location-pages")]
public sealed class LocationSeoPagesController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LocationSeoPage>>> GetPublished(CancellationToken ct)
        => Ok(await db.LocationSeoPages.AsNoTracking().Where(p => p.IsPublished)
            .OrderBy(p => p.Country).ThenBy(p => p.City).ToListAsync(ct));

    [HttpGet("status")]
    public async Task<IActionResult> GetRouteStatuses(CancellationToken ct)
        => Ok(await db.LocationSeoPages.AsNoTracking().Select(p => new
        {
            p.CountrySlug, p.CitySlug, p.IsPublished, p.IsIndexable
        }).ToListAsync(ct));
    [HttpGet("{countrySlug}/{citySlug}")]
    public async Task<IActionResult> GetPublishedBySlug(string countrySlug, string citySlug, CancellationToken ct)
    {
        var page = await db.LocationSeoPages.AsNoTracking().FirstOrDefaultAsync(
            p => p.CountrySlug == countrySlug && p.CitySlug == citySlug, ct);
        if (page is null) return NotFound();
        if (!page.IsPublished) return StatusCode(StatusCodes.Status410Gone);
        return Ok(page);
    }
}

[ApiController]
[Authorize(Roles = "Admin")]
[Route("api/admin/location-pages")]
public sealed class AdminLocationSeoPagesController(ApplicationDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<LocationSeoPage>>> GetAll(CancellationToken ct)
        => Ok(await db.LocationSeoPages.AsNoTracking().OrderBy(p => p.Country).ThenBy(p => p.City).ToListAsync(ct));

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken ct)
    {
        var page = await db.LocationSeoPages.FindAsync([id], ct);
        return page is null ? NotFound() : Ok(page);
    }

    [HttpPost]
    public async Task<IActionResult> Create(LocationSeoPageWriteDto dto, CancellationToken ct)
    {
        var conflict = await db.LocationSeoPages.AnyAsync(p => p.CountrySlug == dto.CountrySlug && p.CitySlug == dto.CitySlug, ct);
        if (conflict) return Conflict(new { message = "A page already uses this country and city URL." });
        var page = new LocationSeoPage();
        Apply(page, dto);
        await db.LocationSeoPages.AddAsync(page, ct);
        await db.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetById), new { id = page.Id }, page);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, LocationSeoPageWriteDto dto, CancellationToken ct)
    {
        var page = await db.LocationSeoPages.FindAsync([id], ct);
        if (page is null) return NotFound();
        var conflict = await db.LocationSeoPages.AnyAsync(p => p.Id != id && p.CountrySlug == dto.CountrySlug && p.CitySlug == dto.CitySlug, ct);
        if (conflict) return Conflict(new { message = "A page already uses this country and city URL." });
        Apply(page, dto);
        page.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return Ok(page);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken ct)
    {
        var page = await db.LocationSeoPages.FindAsync([id], ct);
        if (page is null) return NotFound();
        db.LocationSeoPages.Remove(page);
        await db.SaveChangesAsync(ct);
        return NoContent();
    }

    private static void Apply(LocationSeoPage page, LocationSeoPageWriteDto dto)
    {
        page.Country = dto.Country.Trim(); page.CountryCode = dto.CountryCode.Trim().ToUpperInvariant();
        page.CountrySlug = dto.CountrySlug.Trim().ToLowerInvariant(); page.QuoteCurrency = dto.QuoteCurrency.Trim().ToUpperInvariant();
        page.City = dto.City.Trim(); page.Region = dto.Region.Trim(); page.CitySlug = dto.CitySlug.Trim().ToLowerInvariant();
        page.MetaTitle = dto.MetaTitle.Trim(); page.MetaDescription = dto.MetaDescription.Trim(); page.Heading = dto.Heading.Trim();
        page.Intro = dto.Intro.Trim(); page.BuyerFocus = dto.BuyerFocus.Trim(); page.IsPublished = dto.IsPublished;
        page.IsIndexable = dto.IsIndexable;
    }
}

