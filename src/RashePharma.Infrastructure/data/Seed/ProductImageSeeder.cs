using Microsoft.EntityFrameworkCore;
using RashePharma.Domain.Entities;

namespace RashePharma.Infrastructure.Data.Seed;

public static class ProductImageSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext db,
        CancellationToken cancellationToken = default)
    {
        var products = await db.Products
            .AsNoTracking()
            .Select(p => new
            {
                p.Id,
                p.Name,
                p.Slug
            })
            .ToListAsync(cancellationToken);

        foreach (var product in products)
        {
            var hasImage = await db.ProductImages
                .AnyAsync(
                    image => image.ProductId == product.Id,
                    cancellationToken);

            if (hasImage)
                continue;

            var image = new ProductImage
            {
                ProductId = product.Id,
                ImageUrl = $"/images/products/{product.Slug}.svg",
                AltText = product.Name,
                IsPrimary = true,
                DisplayOrder = 1
            };

            await db.ProductImages.AddAsync(
                image,
                cancellationToken);
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}