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
        // =========================================================
        // Get Admin Role
        // =========================================================

        var adminRole = await db.Roles
            .FirstOrDefaultAsync(
                role => role.Name == "Admin",
                cancellationToken);

        if (adminRole == null)
        {
            throw new InvalidOperationException(
                "Admin role does not exist. Seed Roles before AdminSeeder.");
        }

        // =========================================================
        // Admin credentials from environment
        // =========================================================

        var adminEmail =
            Environment.GetEnvironmentVariable("ADMIN_EMAIL");

        var adminPassword =
            Environment.GetEnvironmentVariable("ADMIN_PASSWORD");

        if (string.IsNullOrWhiteSpace(adminEmail))
        {
            throw new InvalidOperationException(
                "ADMIN_EMAIL is not configured.");
        }

        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException(
                "ADMIN_PASSWORD is not configured.");
        }

        // =========================================================
        // Check if admin user already exists
        // =========================================================

        var existingUser = await db.Users
            .FirstOrDefaultAsync(
                user => user.Email == adminEmail,
                cancellationToken);

        if (existingUser != null)
        {
            // Do not overwrite an existing user.
            return;
        }

        // =========================================================
        // Create Admin User
        // =========================================================

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

        // =========================================================
        // Hash Admin Password
        // =========================================================

        var passwordHasher = new PasswordHasher<User>();

        adminUser.PasswordHash =
            passwordHasher.HashPassword(
                adminUser,
                adminPassword);

        await db.Users.AddAsync(
            adminUser,
            cancellationToken);

        await db.SaveChangesAsync(cancellationToken);
    }
}