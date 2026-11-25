using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RashePharma.Infrastructure.Data;
using RashePharma.Infrastructure.Data.Seed;

var builder = Host.CreateApplicationBuilder(Array.Empty<string>());

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection is required by the database migrator.");
}

builder.Services.AddDbContext<ApplicationDbContext>(
    options => options.UseSqlServer(connectionString));

using var host = builder.Build();
using var scope = host.Services.CreateScope();

var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

Console.WriteLine("Applying pending EF Core migrations...");
await db.Database.MigrateAsync();
Console.WriteLine("Database migrations completed.");

var shouldSeed = args.Contains(
    "--seed",
    StringComparer.OrdinalIgnoreCase);

if (shouldSeed)
{
    Console.WriteLine("Running bootstrap seeders...");
    await RoleSeeder.SeedAsync(db);
    await AdminSeeder.SeedAsync(db);
    Console.WriteLine("Bootstrap seeding completed.");
}
else
{
    Console.WriteLine("Bootstrap seeders skipped. Pass --seed to create required roles/admin.");
}
