using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using RashePharma.Infrastructure.Data;

namespace RashePharma.PostgresMigrations.Data;

public class PostgresDbContextFactory
    : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var connectionString =
            Environment.GetEnvironmentVariable(
                "ConnectionStrings__DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException(
                "ConnectionStrings__DefaultConnection is not configured.");
        }

        var optionsBuilder =
            new DbContextOptionsBuilder<ApplicationDbContext>();

        optionsBuilder.UseNpgsql(
            connectionString,
            options =>
            {
                options.MigrationsAssembly(
                    typeof(PostgresDbContextFactory).Assembly.FullName);
            });

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}