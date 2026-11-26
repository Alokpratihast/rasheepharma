using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RashePharma.Domain.Entities;

namespace RashePharma.Infrastructure.Data.Seed;

public static class AdminSeeder
{
    public static async Task SeedAsync(
        ApplicationDbContext db,
        CancellationToken cancellationToken = default)
    {
        var adminRole = await db.Roles
            .SingleOrDefaultAsync(
                role => role.Name == "Admin",
                cancellationToken);

        if (adminRole == null)
        {
            throw new InvalidOperationException(
                "Admin role does not exist. Seed roles before running AdminSeeder.");
        }

        var adminEmail = Environment.GetEnvironmentVariable("ADMIN_EMAIL")?.Trim();

        if (string.IsNullOrWhiteSpace(adminEmail))
        {
            throw new InvalidOperationException(
                "ADMIN_EMAIL is required to identify the bootstrap admin account.");
        }

        // Check the account before requiring a password. Re-running bootstrap
        // must not rotate credentials or modify an existing user's profile.
        var existingUser = await db.Users
            .SingleOrDefaultAsync(
                user => user.Email == adminEmail,
                cancellationToken);

        if (existingUser != null)
        {
            if (existingUser.RoleId != adminRole.Id)
            {
                throw new InvalidOperationException(
                    $"The configured admin email '{adminEmail}' already belongs to a non-admin user. " +
                    "No role or account data was changed. Resolve this account explicitly before retrying.");
            }

            return;
        }

        var adminPassword = Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException(
                "ADMIN_PASSWORD is required when the bootstrap admin account does not exist.");
        }

        var adminUser = new User
        {
            FirstName = "Admin",
            LastName = "RashePharma",
            Email = adminEmail,
            PhoneNumber = null,
            Country = "India",
            RoleId = adminRole.Id,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var passwordHasher = new PasswordHasher<User>();
        adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, adminPassword);

        await db.Users.AddAsync(adminUser, cancellationToken);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            // The unique Users.Email index is the final concurrency guard.
            // If another bootstrap process won the insert, accept it only when
            // the persisted account is actually assigned to the Admin role.
            db.Entry(adminUser).State = EntityState.Detached;

            var persistedUser = await db.Users
                .SingleOrDefaultAsync(
                    user => user.Email == adminEmail,
                    cancellationToken);

            if (persistedUser?.RoleId == adminRole.Id)
            {
                return;
            }

            throw;
        }
    }
}
