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

        var existingRoleNames = await db.Roles
            .Where(role => requiredRoles.Contains(role.Name))
            .Select(role => role.Name)
            .ToListAsync(cancellationToken);

        foreach (var roleName in requiredRoles)
        {
            if (!existingRoleNames.Contains(roleName, StringComparer.OrdinalIgnoreCase))
            {
                await db.Roles.AddAsync(
                    new Role
                    {
                        Name = roleName
                    },
                    cancellationToken);
            }
        }

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // A concurrent seeder may have inserted the same role after our
            // existence check. Detach pending inserts before verifying the winner.
            foreach (var entry in db.ChangeTracker
                         .Entries<Role>()
                         .Where(entry => entry.State == EntityState.Added)
                         .ToList())
            {
                entry.State = EntityState.Detached;
            }

            var persistedRoleNames = await db.Roles
                .Where(role => requiredRoles.Contains(role.Name))
                .Select(role => role.Name)
                .ToListAsync(cancellationToken);

            var allRequiredRolesExist = requiredRoles.All(
                requiredRole => persistedRoleNames.Contains(
                    requiredRole,
                    StringComparer.OrdinalIgnoreCase));

            if (!allRequiredRolesExist)
            {
                throw;
            }
        }
    }
}
