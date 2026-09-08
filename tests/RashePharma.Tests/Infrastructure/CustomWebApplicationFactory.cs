using System.Data.Common;
using System.Security.Claims;
using System.Text.Encodings.Web;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using RashePharma.Infrastructure.Data;

namespace RashePharma.Tests.Infrastructure;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private DbConnection? _connection;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // ---------------------------------------------------------
            // Replace ApplicationDbContext with SQLite in-memory DB
            // ---------------------------------------------------------

            var dbContextDescriptor = services
                .SingleOrDefault(
                    d => d.ServiceType ==
                         typeof(DbContextOptions<ApplicationDbContext>));

            if (dbContextDescriptor != null)
            {
                services.Remove(dbContextDescriptor);
            }

            _connection = new SqliteConnection(
                "DataSource=:memory:");

            _connection.Open();

            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });


            // ---------------------------------------------------------
            // Keep real JWT authentication
            //
            // Also support TestAuthenticationHandler for tests that
            // send X-Test-UserId / X-Test-Role headers.
            // ---------------------------------------------------------

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme =
                        "TestOrJwt";

                    options.DefaultChallengeScheme =
                        "TestOrJwt";
                })
                .AddPolicyScheme(
                    "TestOrJwt",
                    "Test authentication or JWT authentication",
                    options =>
                    {
                        options.ForwardDefaultSelector = context =>
                        {
                            if (context.Request.Headers.ContainsKey(
                                "X-Test-UserId"))
                            {
                                return TestAuthenticationHandler.SchemeName;
                            }

                            return JwtBearerDefaults.AuthenticationScheme;
                        };
                    })
                .AddScheme<AuthenticationSchemeOptions,
                    TestAuthenticationHandler>(
                    TestAuthenticationHandler.SchemeName,
                    _ => { });


            // ---------------------------------------------------------
            // Create test database
            // ---------------------------------------------------------

            var serviceProvider =
                services.BuildServiceProvider();

            using var scope =
                serviceProvider.CreateScope();

            var db =
                scope.ServiceProvider
                    .GetRequiredService<ApplicationDbContext>();

            db.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _connection?.Dispose();
        }

        base.Dispose(disposing);
    }
}


// ================================================================
// Test Authentication Handler
// ================================================================

public class TestAuthenticationHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";

    public TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // ---------------------------------------------------------
        // Read test identity from request headers
        // ---------------------------------------------------------

        if (!Request.Headers.TryGetValue(
                "X-Test-UserId",
                out var userIdHeader))
        {
            return Task.FromResult(
                AuthenticateResult.NoResult());
        }

        if (!int.TryParse(
                userIdHeader.ToString(),
                out var userId))
        {
            return Task.FromResult(
                AuthenticateResult.Fail(
                    "Invalid test user id."));
        }

        var claims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                userId.ToString())
        };


        // ---------------------------------------------------------
        // Optional role
        // ---------------------------------------------------------

        if (Request.Headers.TryGetValue(
                "X-Test-Role",
                out var roleHeader))
        {
            var role = roleHeader.ToString();

            if (!string.IsNullOrWhiteSpace(role))
            {
                claims.Add(
                    new Claim(
                        ClaimTypes.Role,
                        role));
            }
        }


        // ---------------------------------------------------------
        // Create authenticated identity
        // ---------------------------------------------------------

        var identity = new ClaimsIdentity(
            claims,
            SchemeName);

        var principal =
            new ClaimsPrincipal(identity);

        var ticket =
            new AuthenticationTicket(
                principal,
                SchemeName);

        return Task.FromResult(
            AuthenticateResult.Success(ticket));
    }
}