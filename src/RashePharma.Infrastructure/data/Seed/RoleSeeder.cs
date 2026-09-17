using Microsoft.EntityFrameworkCore;
using RashePharma.Domain.Entities;

namespace RashePharma.Infrastructure.Data.Seed;

public static class RoleSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext db,
        CancellationToken cancellationToken = default)
    {
        var requiredRoles = new[]
        {
            "User",
            "Admin"
        };

        foreach (var roleName in requiredRoles)
        {
            var exists = await db.Roles
                .AnyAsync(
                    role => role.Name == roleName,
                    cancellationToken);

            if (!exists)
            {
                await db.Roles.AddAsync(
                    new Role
                    {
                        Name = roleName
                    },
                    cancellationToken);
            }
        }

        await db.SaveChangesAsync(cancellationToken);
    }
}