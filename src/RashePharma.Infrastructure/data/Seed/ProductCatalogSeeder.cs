using Microsoft.EntityFrameworkCore;
using RashePharma.Domain.Entities;
using RashePharma.Infrastructure.Data;

namespace RashePharma.Infrastructure.Data.Seed;

/// <summary>
/// Bootstrap seed for the current Rashe Lifesciences website catalogue.
/// Creates the initial 7 catalogue categories and 33 products.
/// Missing catalogue records are created only; existing records are not overwritten.
/// Price/stock/image data is only populated where verified from the source.
/// </summary>
public static class ProductCatalogSeeder
{
    private sealed record CatalogItem(
        string CategoryName,
        string CategorySlug,
        string Name,
        string Form,
        string? Composition = null,
        string? Strength = null,
        string? PackSize = null,
        decimal Price = 0m,
        string? Currency = null,
        int? MOQ = null,
        string? UnitType = null,
        string? BrandName = null);

    private static readonly CatalogItem[] Items =
    {
        // Pharmaceutical Tablets
        new("Pharmaceutical Tablets", "pharmaceutical-tablets", "Aceclin P Tablets",
            "Tablet", "Aceclofenac and Paracetamol Tablets", MOQ: 100, UnitType: "Pack"),
        new("Pharmaceutical Tablets", "pharmaceutical-tablets", "AZR Tablets",
            "Tablet", "Azithromycin Tablets IP", "500 MG", Price: 1m, Currency: "USD", MOQ: 100, UnitType: "Pack"),
        new("Pharmaceutical Tablets", "pharmaceutical-tablets", "Cialis Tablets",
            "Tablet", "Tadalafil Tablets IP", "10 mg", Price: 7m, Currency: "USD", PackSize: "Pack of 2", MOQ: 100, UnitType: "Pack", BrandName: "Lupin"),
        new("Pharmaceutical Tablets", "pharmaceutical-tablets", "Digiver Tablets",
            "Tablet", "Ivermectin Tablets", "12 mg", Price: 4m, Currency: "USD", UnitType: "Strip", MOQ: 100, BrandName: "Digital Vision"),
        new("Pharmaceutical Tablets", "pharmaceutical-tablets", "Edsave Tablets",
            "Tablet", "Tadalafil Tablets IP", "5 mg", Price: 2m, Currency: "USD", UnitType: "Strip", MOQ: 100, BrandName: "Fourrtx"),
        new("Pharmaceutical Tablets", "pharmaceutical-tablets", "Farovib Tablets",
            "Tablet", "Faropenem Tablets", "200 mg", Price: 30m, Currency: "USD", UnitType: "Strip", MOQ: 100),
        new("Pharmaceutical Tablets", "pharmaceutical-tablets", "Favitru Tablets",
            "Tablet", "Favipiravir Tablets", "400 mg", Price: 30m, Currency: "USD", UnitType: "Strip", MOQ: 100),
        new("Pharmaceutical Tablets", "pharmaceutical-tablets", "Fungicip Tablets",
            "Tablet", "Fluconazole Tablets IP", "200 mg", Price: 3m, Currency: "USD", UnitType: "Strip", MOQ: 100, BrandName: "Cipla"),
        new("Pharmaceutical Tablets", "pharmaceutical-tablets", "HCQS Tablets",
            "Tablet", "Hydroxychloroquine Tablets IP", Price: 2m, Currency: "USD", UnitType: "Strip", MOQ: 100, BrandName: "Ipca"),
        new("Pharmaceutical Tablets", "pharmaceutical-tablets", "HYQ Tablets",
            "Tablet", "Hydroxychloroquine Tablets IP", "400 mg", Price: 2m, Currency: "USD", UnitType: "Strip", MOQ: 100, BrandName: "Ipca"),
        new("Pharmaceutical Tablets", "pharmaceutical-tablets", "Ivervib Tablets",
            "Tablet", "Ivermectin Dispersible Tablets", "12 mg", Price: 2m, Currency: "USD", UnitType: "Strip", MOQ: 100, BrandName: "Vibcare Pharma Pvt Ltd"),
        new("Pharmaceutical Tablets", "pharmaceutical-tablets", "Manforce Tablets",
            "Tablet", "Sildenafil Tablets IP", "100 Mg", Price: 2m, Currency: "USD", UnitType: "Strip", MOQ: 100, BrandName: "Mankind"),
        new("Pharmaceutical Tablets", "pharmaceutical-tablets", "Megalis Tablets",
            "Tablet", "Tadalafil Tablets IP", "20 Mg", Price: 3m, Currency: "USD", UnitType: "Strip", MOQ: 100, BrandName: "Macleods"),
        new("Pharmaceutical Tablets", "pharmaceutical-tablets", "Suhagra Tablets",
            "Tablet", "Sildenafil Citrate Tablets IP", "100 Mg", Price: 3m, Currency: "USD", PackSize: "4 tablets", UnitType: "Strip", MOQ: 100, BrandName: "Cipla"),
        new("Pharmaceutical Tablets", "pharmaceutical-tablets", "Tadacip Tablets",
            "Tablet", "Tadalafil Tablets IP", "20 mg", Price: 2m, Currency: "USD", UnitType: "Strip", MOQ: 100, BrandName: "Cipla"),
        new("Pharmaceutical Tablets", "pharmaceutical-tablets", "Tadact Tablets",
            "Tablet", "Tadalafil Tablets IP", "10 mg", Price: 3m, Currency: "USD", UnitType: "Strip", MOQ: 100, BrandName: "Ipca"),

        // Pharmaceutical Capsules
        new("Pharmaceutical Capsules", "pharmaceutical-capsules", "Aerocort Forte Rotacaps",
            "Rotacaps"),
        new("Pharmaceutical Capsules", "pharmaceutical-capsules", "Doxtra LB Capsules",
            "Capsules", "Doxycycline Hydrochloride & Lactic Acid Bacillus Capsules", PackSize: "20 Capsules", MOQ: 100, UnitType: "Pack", BrandName: "Xtrakind"),
        new("Pharmaceutical Capsules", "pharmaceutical-capsules", "Goldaris Super Power Capsules",
            "Capsules"),
        new("Pharmaceutical Capsules", "pharmaceutical-capsules", "Rabzaris DSR Capsules",
            "Capsules"),
        new("Pharmaceutical Capsules", "pharmaceutical-capsules", "RL-Fit Softgel Capsules",
            "Softgel Capsules"),
        new("Pharmaceutical Capsules", "pharmaceutical-capsules", "Rosuvib Gold Capsules",
            "Capsules"),
        new("Pharmaceutical Capsules", "pharmaceutical-capsules", "Troycon Capsules",
            "Capsules"),

        // Pharmaceutical Injection
        new("Pharmaceutical Injection", "pharmaceutical-injection", "Evaparin PFS Injection",
            "Injection", MOQ: 100, UnitType: "Piece"),
        new("Pharmaceutical Injection", "pharmaceutical-injection", "Megmanox Injection",
            "Injection", MOQ: 100, UnitType: "Piece"),
        new("Pharmaceutical Injection", "pharmaceutical-injection", "Multilan C Injection",
            "Injection", MOQ: 100, UnitType: "Piece"),

        // Pharmaceutical Nasal Spray
        new("Pharmaceutical Nasal Spray", "pharmaceutical-nasal-spray", "Hitop Nasal Spray",
            "Liquid Drop", "Fluticasone Nasal Spray IP", Price: 2.30m, Currency: "USD",
            MOQ: 100, UnitType: "Piece", PackSize: "Plastic Bottle", BrandName: "Max"),

        // Pharmaceutical Respules
        new("Pharmaceutical Respules", "pharmaceutical-respules", "Budovib Respules",
            "Respules", "Budesonide 0.5mg (Respirator Suspension)", "0.5mg",
            "4x5x2 ml", 1.20m, "USD", 100, "Pack", "Vibcare Pharma Pvt Ltd"),
        new("Pharmaceutical Respules", "pharmaceutical-respules", "Iptrovib Respules",
            "Respules"),

        // Pharmaceutical Cream & Gel
        new("Pharmaceutical Cream & Gel", "pharmaceutical-cream-gel", "Acnetroy Gel",
            "Gel", "Clindamycin Phosphate and Nicotinamide Gel", PackSize: "20 gm",
            Price: 1.50m, Currency: "USD", MOQ: 10, UnitType: "Piece"),
        new("Pharmaceutical Cream & Gel", "pharmaceutical-cream-gel", "Skinlite Cream",
            "Cream", "Hydroquinone + Tretinoin + Mometasone Furoate Cream", PackSize: "10 gm",
            Price: 3m, Currency: "USD", MOQ: 100, UnitType: "Pack", BrandName: "Zydus"),
        new("Pharmaceutical Cream & Gel", "pharmaceutical-cream-gel", "Terbeez Cream",
            "Cream", "Terbinafine Hydrochloride Cream IP", PackSize: "10 gm",
            Price: 2m, Currency: "USD", MOQ: 100, UnitType: "Piece"),

        // Other Products
        new("Other Products", "other-products", "Tadarise Oral Jelly",
            "Jelly", "Tadalafil oral jelly 20 mg", "20 Mg", PackSize: "Pack of 10",
            Price: 2m, Currency: "USD", MOQ: 100, UnitType: "Pack")
    };

    public static async Task SeedAsync(
        ApplicationDbContext db,
        CancellationToken cancellationToken = default)
    {
        // This is a bootstrap seeder. Once products exist, normal admin/database
        // management owns the catalogue and this seeder does not overwrite it.
        if (await db.Products.AnyAsync(cancellationToken))
            return;

        await using var transaction =
            await db.Database.BeginTransactionAsync(cancellationToken);

        var now = DateTime.UtcNow;

        var categoriesBySlug = await db.Categories
            .ToDictionaryAsync(c => c.Slug, cancellationToken);

        foreach (var item in Items)
        {
            if (categoriesBySlug.ContainsKey(item.CategorySlug))
                continue;

            var category = new Category
            {
                Name = item.CategoryName,
                Slug = item.CategorySlug,
                Description = null,
                IsActive = true,
                ParentCategoryId = null,
                CreatedAt = now
            };

            db.Categories.Add(category);
            categoriesBySlug[item.CategorySlug] = category;
        }

        await db.SaveChangesAsync(cancellationToken);

        foreach (var item in Items)
        {
            var category = categoriesBySlug[item.CategorySlug];

            var productSlug = Slugify(
                $"{item.CategorySlug}-{item.Name}");

            var product = new Product
            {
                Name = item.Name,
                Slug = productSlug,
                GenericName = null,
                Composition = item.Composition,
                DosageForm = item.Form,
                Description = null,
                BrandName = item.BrandName,
                Manufacturer = null,
                CategoryId = category.Id,
                IsActive = true,
                CreatedAt = now
            };

            db.Products.Add(product);
        }

        await db.SaveChangesAsync(cancellationToken);

        foreach (var item in Items)
        {
            var productSlug = Slugify(
                $"{item.CategorySlug}-{item.Name}");

            var product = await db.Products
                .SingleAsync(
                    p => p.Slug == productSlug,
                    cancellationToken);

            db.ProductVariants.Add(new ProductVariant
            {
                ProductId = product.Id,
                Strength = item.Strength,
                PackSize = item.PackSize,
                Price = item.Price,
                Currency = item.Currency,
                MOQ = item.MOQ,
                UnitType = item.UnitType,
                SKU = null,
                StockQuantity = 0,
                IsActive = true,
                CreatedAt = now
            });
        }

        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private static string Slugify(string value)
    {
        var chars = value
            .ToLowerInvariant()
            .Select(ch => char.IsLetterOrDigit(ch) ? ch : '-')
            .ToArray();

        var slug = new string(chars);

        while (slug.Contains("--", StringComparison.Ordinal))
        {
            slug = slug.Replace(
                "--",
                "-",
                StringComparison.Ordinal);
        }

        return slug.Trim('-');
    }
}
