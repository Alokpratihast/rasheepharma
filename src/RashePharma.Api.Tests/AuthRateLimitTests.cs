using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using RashePharma.Api.Controllers;
using RashePharma.Application.DTOs.Auth;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Infrastructure.Data;
using RashePharma.Infrastructure.Services;
using Xunit;

namespace RashePharma.Api.Tests;

public sealed class AuthRateLimitTests : IClassFixture<AuthApiFactory>
{
    private readonly HttpClient _client;

    public AuthRateLimitTests(AuthApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Login_returns_429_after_ten_attempts_from_same_client()
    {
        var statuses = new List<HttpStatusCode>();

        for (var attempt = 0; attempt < 11; attempt++)
        {
            using var response = await _client.PostAsJsonAsync(
                "/api/Auth/login",
                new LoginDto { Email = "customer@example.com", Password = "wrong-password" });
            statuses.Add(response.StatusCode);
        }

        Assert.All(statuses.Take(10), status => Assert.Equal(HttpStatusCode.Unauthorized, status));
        Assert.Equal(HttpStatusCode.TooManyRequests, statuses[10]);
    }

    [Fact]
    public async Task Password_recovery_limit_is_shared_by_forgot_and_reset_endpoints()
    {
        for (var attempt = 0; attempt < 5; attempt++)
        {
            using var response = await _client.PostAsJsonAsync(
                "/api/Auth/forgot-password",
                new ForgotPasswordDto { Email = "customer@example.com" });
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        using var limitedResponse = await _client.PostAsJsonAsync(
            "/api/Auth/reset-password",
            new ResetPasswordDto { Token = "reset-token", NewPassword = "a-valid-password" });

        Assert.Equal(HttpStatusCode.TooManyRequests, limitedResponse.StatusCode);
    }

    [Fact]
    public async Task Refresh_returns_429_after_thirty_requests_from_same_client()
    {
        var statuses = new List<HttpStatusCode>();

        for (var attempt = 0; attempt < 31; attempt++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/Auth/refresh");
            request.Headers.Add("Origin", "https://frontend.example");
            using var response = await _client.SendAsync(request);
            statuses.Add(response.StatusCode);
        }

        Assert.All(statuses.Take(30), status => Assert.Equal(HttpStatusCode.Forbidden, status));
        Assert.Equal(HttpStatusCode.TooManyRequests, statuses[30]);
    }
}

public sealed class AuthApiFactory : WebApplicationFactory<Program>
{
    private readonly string? _previousStripeKey = Environment.GetEnvironmentVariable("Stripe__SecretKey");
    private readonly string? _previousJwtKey = Environment.GetEnvironmentVariable("JWT__KEY");

    public AuthApiFactory()
    {
        // Program reads these deployment secrets during startup; use test-only values for this host.
        Environment.SetEnvironmentVariable("Stripe__SecretKey", "sk_test_auth_rate_limit");
        Environment.SetEnvironmentVariable("JWT__KEY", new string('t', 64));
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configuration) =>
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "auth-tests",
                ["Jwt:Audience"] = "auth-tests"
            }));

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IAuthService>();
            services.AddSingleton<IAuthService, StubAuthService>();
            services.AddDbContext<ApplicationDbContext>();
            services.RemoveAll<IHostedService>();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (disposing)
        {
            Environment.SetEnvironmentVariable("Stripe__SecretKey", _previousStripeKey);
            Environment.SetEnvironmentVariable("JWT__KEY", _previousJwtKey);
        }
    }
}

internal sealed class StubAuthService : IAuthService
{
    public Task<AuthResultDto> RegisterAsync(RegisterDto dto) =>
        Task.FromResult(CreateResult());

    public Task<AuthResultDto?> LoginAsync(LoginDto dto) =>
        Task.FromResult<AuthResultDto?>(null);

    public Task<UserProfileDto?> GetProfileAsync(int userId) =>
        Task.FromResult<UserProfileDto?>(null);

    public Task<AuthResultDto?> RefreshAsync(string refreshToken) =>
        Task.FromResult<AuthResultDto?>(null);

    public Task LogoutAsync(string refreshToken) => Task.CompletedTask;

    public Task ForgotPasswordAsync(string email) => Task.CompletedTask;

    public Task<bool> ResetPasswordAsync(string token, string newPassword) =>
        Task.FromResult(false);

    private static AuthResultDto CreateResult() => new()
    {
        RefreshToken = "test-refresh-token",
        Response = new AuthResponseDto { UserId = 1, Email = "customer@example.com", Role = "User" }
    };
}