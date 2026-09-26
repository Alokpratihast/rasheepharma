using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Application.Services;
using RashePharma.Infrastructure.Data;
using RashePharma.Infrastructure.Data.Seed;
using RashePharma.Infrastructure.Repositories;
using RashePharma.Infrastructure.Services;
using System.Text;
using Stripe;

// =========================================================
// Create application builder
// =========================================================

var builder = WebApplication.CreateBuilder(args);

StripeConfiguration.ApiKey =
    builder.Configuration["Stripe:SecretKey"];

// =========================================================
// Load .env for local development only
// =========================================================

if (builder.Environment.IsDevelopment())
{
    var currentDirectory = new DirectoryInfo(
        AppContext.BaseDirectory);

    var rootDirectory = currentDirectory;

    while (rootDirectory != null)
    {
        var envFilePath = Path.Combine(
            rootDirectory.FullName,
            ".env");

        if (System.IO.File.Exists(envFilePath))
        {
            Env.Load(envFilePath);
            break;
        }

        rootDirectory = rootDirectory.Parent;
    }
}

// =========================================================
// Stripe
// =========================================================

var stripeSecretKey =
    Environment.GetEnvironmentVariable("Stripe__SecretKey");

if (string.IsNullOrWhiteSpace(stripeSecretKey))
{
    throw new InvalidOperationException(
        "Stripe__SecretKey is not configured.");
}

builder.Configuration["Stripe:SecretKey"] =
    stripeSecretKey;

// Webhook Secret
var stripeWebhookSecret =
    Environment.GetEnvironmentVariable("Stripe__WebhookSecret");

if (!string.IsNullOrWhiteSpace(stripeWebhookSecret))
{
    builder.Configuration["Stripe:WebhookSecret"] =
        stripeWebhookSecret;
}

// =========================================================
// Environment Variables
// =========================================================

// JWT Key
var jwtKeyFromEnv =
    Environment.GetEnvironmentVariable("JWT__KEY");

if (!string.IsNullOrWhiteSpace(jwtKeyFromEnv))
{
    builder.Configuration["Jwt:Key"] =
        jwtKeyFromEnv;
}

// Database Connection String
var connectionStringFromEnv =
    Environment.GetEnvironmentVariable(
        "ConnectionStrings__DefaultConnection");

if (!string.IsNullOrWhiteSpace(connectionStringFromEnv))
{
    builder.Configuration[
        "ConnectionStrings:DefaultConnection"] =
        connectionStringFromEnv;
}

// Azure Storage Connection String
var azureStorageConnectionString =
    Environment.GetEnvironmentVariable(
        "ConnectionStrings__AzureStorage");

if (!string.IsNullOrWhiteSpace(azureStorageConnectionString))
{
    builder.Configuration[
        "ConnectionStrings:AzureStorage"] =
        azureStorageConnectionString;
}

// =========================================================
// Controllers / API
// =========================================================

builder.Services.AddControllers();

builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen();

// =========================================================
// CORS
// =========================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var allowedOrigins =
            builder.Configuration
                .GetSection("Cors:AllowedOrigins")
                .Get<string[]>();

        if (allowedOrigins != null &&
            allowedOrigins.Length > 0)
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
        else if (builder.Environment.IsDevelopment())
        {
            policy
                .WithOrigins(
                    "http://localhost:3000",
                    "https://rasheepharma.vercel.app")
                .AllowAnyHeader()
                .AllowAnyMethod();
        }
    });
});

// =========================================================
// Database
// =========================================================

if (!builder.Environment.IsEnvironment("Testing"))
{
    var connectionString =
        builder.Configuration.GetConnectionString(
            "DefaultConnection");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "ConnectionStrings:DefaultConnection is not configured.");
    }

    // -----------------------------------------------------
    // SQL Server
    // Local Development + Staging + Production
    // Azure SQL compatible
    // -----------------------------------------------------

    builder.Services.AddDbContext<ApplicationDbContext>(
        options =>
            options.UseSqlServer(connectionString!));
}

// =========================================================
// JWT Authentication
// =========================================================

var jwtSettings =
    builder.Configuration.GetSection("Jwt");

var jwtKey =
    jwtSettings["Key"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "JWT Key is not configured.");
}

var jwtIssuer =
    jwtSettings["Issuer"];

if (string.IsNullOrWhiteSpace(jwtIssuer))
{
    throw new InvalidOperationException(
        "JWT Issuer is not configured.");
}

var jwtAudience =
    jwtSettings["Audience"];

if (string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException(
        "JWT Audience is not configured.");
}

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
// Dependency Injection
// =========================================================

// Unit of Work
builder.Services.AddScoped<
    IUnitOfWork,
    UnitOfWork>();

// ---------------------------------------------------------
// Repositories
// ---------------------------------------------------------

builder.Services.AddScoped<
    IProductRepository,
    ProductRepository>();

builder.Services.AddScoped<
    ICategoryRepository,
    CategoryRepository>();

builder.Services.AddScoped<
    IProductImageRepository,
    ProductImageRepository>();

builder.Services.AddScoped<
    IProductVariantRepository,
    ProductVariantRepository>();

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

builder.Services.AddScoped<
    IWebsiteContentRepository,
    WebsiteContentRepository>();

// ---------------------------------------------------------
// Services
// ---------------------------------------------------------

builder.Services.AddScoped<
    IProductService,
    RashePharma.Application.Services.ProductService>();

builder.Services.AddScoped<
    IProductVariantService,
    ProductVariantService>();

builder.Services.AddScoped<
    IProductImageService,
    ProductImageService>();

builder.Services.AddScoped<IImageStorageService>(sp =>
{
    var configuration =
        sp.GetRequiredService<IConfiguration>();

    var connectionString =
        configuration.GetConnectionString("AzureStorage");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "ConnectionStrings:AzureStorage is not configured.");
    }

    return new AzureBlobImageStorageService(
        connectionString);
});

builder.Services.AddScoped<
    ICategoryService,
    CategoryService>();

builder.Services.AddScoped<
    IWebsiteContentService,
    WebsiteContentService>();

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
    IPaymentService,
    PaymentService>();

builder.Services.AddScoped<
    IEnquiryService,
    EnquiryService>();

builder.Services.AddScoped<
    IQuotationService,
    QuotationService>();

builder.Services.AddScoped<
    IPartnerService,
    PartnerService>();

builder.Services.AddScoped<
    IAdminService,
    AdminService>();

builder.Services.AddScoped<
    IPaymentRepository,
    PaymentRepository>();

// =========================================================
// Build Application
// =========================================================

var app = builder.Build();

// =========================================================
// Database Seed
// =========================================================

if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();

    var db =
        scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

    // Apply pending database migrations
    await db.Database.MigrateAsync();

    // Seed data after database schema is ready
    await RoleSeeder.SeedAsync(db);

    await AdminSeeder.SeedAsync(db);
}

// =========================================================
// Swagger
// Development only
// =========================================================

if (app.Environment.IsDevelopment() ||
    app.Environment.IsStaging())
{
    app.MapOpenApi();

    app.UseSwagger();

    app.UseSwaggerUI();
}

// =========================================================
// Middleware Pipeline
// =========================================================

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseCors("Frontend");

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

// =========================================================
// Run
// =========================================================

app.Run();

// =========================================================
// Partial Program Class
// Required for integration tests
// =========================================================

public partial class Program
{
}