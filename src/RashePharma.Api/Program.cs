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

// =========================================================
// Load .env
// =========================================================

var currentDirectory = new DirectoryInfo(
    AppContext.BaseDirectory);

var rootDirectory = currentDirectory;

while (rootDirectory != null &&
       !File.Exists(
           Path.Combine(rootDirectory.FullName, ".env")))
{
    rootDirectory = rootDirectory.Parent;
}

if (rootDirectory == null)
{
    throw new FileNotFoundException(
        "Root .env file could not be found.");
}

var envFilePath = Path.Combine(
    rootDirectory.FullName,
    ".env");

Env.Load(envFilePath);

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
// CORS
// =========================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins("http://localhost:3000")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

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
    var connectionString =
        builder.Configuration.GetConnectionString(
            "DefaultConnection");

    if (string.IsNullOrWhiteSpace(connectionString))
    {
        throw new InvalidOperationException(
            "ConnectionStrings:DefaultConnection is not configured.");
    }

    builder.Services.AddDbContext<ApplicationDbContext>(
        options =>
            options.UseSqlServer(connectionString));
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

// =========================================================
// Services
// =========================================================

builder.Services.AddScoped<
    IProductService,
    ProductService>();

builder.Services.AddScoped<
    IProductVariantService,
    ProductVariantService>();

builder.Services.AddScoped<
    IProductImageService,
    ProductImageService>();

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
// Database Seed
// =========================================================

if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();

    var db = scope.ServiceProvider
        .GetRequiredService<ApplicationDbContext>();

    await ProductCatalogSeeder.SeedAsync(db);
}

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

// IMPORTANT: CORS must be enabled before
// Authentication / Authorization

app.UseCors("Frontend");

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