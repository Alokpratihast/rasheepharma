using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Application.Services;
using RashePharma.Infrastructure.Data;
using RashePharma.Infrastructure.Repositories;
using RashePharma.Infrastructure.Services;
using System.Text;

// =========================================================
// Load .env
// =========================================================

var envFilePath = Path.Combine(
    Directory.GetCurrentDirectory(),
    ".env");

if (File.Exists(envFilePath))
{
    Env.Load(envFilePath);
}

// =========================================================
// Create application builder
// =========================================================

var builder = WebApplication.CreateBuilder(args);

// =========================================================
// Map .env variables explicitly into ASP.NET configuration
// =========================================================

var jwtKeyFromEnv =
    Environment.GetEnvironmentVariable("JWT__KEY");

if (!string.IsNullOrWhiteSpace(jwtKeyFromEnv))
{
    builder.Configuration["Jwt:Key"] = jwtKeyFromEnv;
}

var connectionStringFromEnv =
    Environment.GetEnvironmentVariable(
        "ConnectionStrings__DefaultConnection");

if (!string.IsNullOrWhiteSpace(connectionStringFromEnv))
{
    builder.Configuration[
        "ConnectionStrings:DefaultConnection"] =
        connectionStringFromEnv;
}

// =========================================================
// Add services to the container
// =========================================================

builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services.AddSwaggerGen();

// =========================================================
// Database
// =========================================================

if (builder.Environment.IsEnvironment("Testing"))
{
    // Test database will be configured by
    // CustomWebApplicationFactory
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(
        options =>
            options.UseSqlServer(
                builder.Configuration.GetConnectionString(
                    "DefaultConnection")));
}

// =========================================================
// JWT Authentication
// =========================================================

var jwtSettings =
    builder.Configuration.GetSection("Jwt");

var jwtKey = jwtSettings["Key"]
    ?? throw new InvalidOperationException(
        "JWT Key is not configured.");

var jwtIssuer = jwtSettings["Issuer"]
    ?? throw new InvalidOperationException(
        "JWT Issuer is not configured.");

var jwtAudience = jwtSettings["Audience"]
    ?? throw new InvalidOperationException(
        "JWT Audience is not configured.");

builder.Services
    .AddAuthentication(
        JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)),

                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                ValidateAudience = true,
                ValidAudience = jwtAudience,

                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero
            };
    });

// =========================================================
// Authorization
// =========================================================

builder.Services.AddAuthorization();

// =========================================================
// Unit of Work
// =========================================================

builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// =========================================================
// Repositories
// =========================================================

builder.Services.AddScoped<
    IProductRepository,
    ProductRepository>();

builder.Services.AddScoped<
    ICategoryRepository,
    CategoryRepository>();

builder.Services.AddScoped<
    IUserRepository,
    UserRepository>();

builder.Services.AddScoped<
    IAddressRepository,
    AddressRepository>();

builder.Services.AddScoped<
    ICartRepository,
    CartRepository>();

builder.Services.AddScoped<
    IOrderRepository,
    OrderRepository>();

builder.Services.AddScoped<
    IEnquiryRepository,
    EnquiryRepository>();

builder.Services.AddScoped<
    IQuotationRepository,
    QuotationRepository>();

builder.Services.AddScoped<
    IPartnerRequestRepository,
    PartnerRequestRepository>();

builder.Services.AddScoped<
    IPartnerRepository,
    PartnerRepository>();

// =========================================================
// Services
// =========================================================

builder.Services.AddScoped<
    IProductService,
    ProductService>();

builder.Services.AddScoped<
    ICategoryService,
    CategoryService>();

builder.Services.AddScoped<
    IAuthService,
    AuthService>();

builder.Services.AddScoped<
    IJwtTokenService,
    JwtTokenService>();

builder.Services.AddScoped<
    IAddressService,
    AddressService>();

builder.Services.AddScoped<
    ICartService,
    CartService>();

builder.Services.AddScoped<
    IOrderService,
    OrderService>();

builder.Services.AddScoped<
    IEnquiryService,
    EnquiryService>();

builder.Services.AddScoped<
    IQuotationService,
    QuotationService>();

builder.Services.AddScoped<
    IPartnerService,
    PartnerService>();

// =========================================================
// Build application
// =========================================================

var app = builder.Build();

// =========================================================
// Swagger / OpenAPI
// =========================================================

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwagger();

    app.UseSwaggerUI();
}

// =========================================================
// Middleware
// =========================================================

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// =========================================================
// Run
// =========================================================

app.Run();

// =========================================================
// Partial Program class
// Required for integration tests
// =========================================================

public partial class Program
{
}