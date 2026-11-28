using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Application.Services;
using RashePharma.Infrastructure.Data;
using RashePharma.Infrastructure.Repositories;
using RashePharma.Infrastructure.Services;
using Microsoft.OpenApi;
using System.Text;
using System.Net;
using QuestPDF.Infrastructure;
using RashePharma.Infrastructure.Workers;
using Stripe;
using System.Threading.RateLimiting;
using RashePharma.Infrastructure;

// =========================================================
// Create application builder
// =========================================================

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024;
});

QuestPDF.Settings.License = LicenseType.Community;

builder.Services.AddMemoryCache();

builder.Services.AddHttpClient<IExchangeRateService, ExchangeRateService>();








// Capture the process-level connection string before loading .env.
// This allows an explicitly supplied CMD/CI value to take priority over .env.
var processConnectionString =
    Environment.GetEnvironmentVariable(
        "ConnectionStrings__DefaultConnection");

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


StripeConfiguration.ApiKey = stripeSecretKey;

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
// Prefer the process value captured before .env was loaded.
// If no process value was supplied, fall back to the value loaded from .env.
var connectionStringFromEnv =
    !string.IsNullOrWhiteSpace(processConnectionString)
        ? processConnectionString
        : Environment.GetEnvironmentVariable(
            "ConnectionStrings__DefaultConnection");

if (!string.IsNullOrWhiteSpace(connectionStringFromEnv))
{
    builder.Configuration[
        "ConnectionStrings:DefaultConnection"] =
        connectionStringFromEnv;
}

// =========================================================
// Frontend
// =========================================================

var frontendBaseUrl =
    Environment.GetEnvironmentVariable(
        "Frontend__BaseUrl");

if (!string.IsNullOrWhiteSpace(frontendBaseUrl))
{
    builder.Configuration["Frontend:BaseUrl"] =
        frontendBaseUrl;
}

// Brevo SMTP
var brevoSmtpHost =
    Environment.GetEnvironmentVariable("Brevo__SmtpHost");

var brevoSmtpPort =
    Environment.GetEnvironmentVariable("Brevo__SmtpPort");

var brevoSmtpUsername =
    Environment.GetEnvironmentVariable("Brevo__SmtpUsername");

var brevoSmtpPassword =
    Environment.GetEnvironmentVariable("Brevo__SmtpPassword");

var brevoFromEmail =
    Environment.GetEnvironmentVariable("Brevo__FromEmail");

var brevoFromName =
    Environment.GetEnvironmentVariable("Brevo__FromName");

if (!string.IsNullOrWhiteSpace(brevoSmtpHost))
{
    builder.Configuration["Brevo:SmtpHost"] =
        brevoSmtpHost;
}

if (!string.IsNullOrWhiteSpace(brevoSmtpPort))
{
    builder.Configuration["Brevo:SmtpPort"] =
        brevoSmtpPort;
}

if (!string.IsNullOrWhiteSpace(brevoSmtpUsername))
{
    builder.Configuration["Brevo:SmtpUsername"] =
        brevoSmtpUsername;
}

if (!string.IsNullOrWhiteSpace(brevoSmtpPassword))
{
    builder.Configuration["Brevo:SmtpPassword"] =
        brevoSmtpPassword;
}

if (!string.IsNullOrWhiteSpace(brevoFromEmail))
{
    builder.Configuration["Brevo:FromEmail"] =
        brevoFromEmail;
}

if (!string.IsNullOrWhiteSpace(brevoFromName))
{
    builder.Configuration["Brevo:FromName"] =
        brevoFromName;
}

// =========================================================
// Controllers / API
// =========================================================

builder.Services.AddControllers();

// Apply endpoint-specific throttles to slow credential guessing and password-reset abuse.
// Limits are partitioned by the network peer address; do not trust forwarded headers unless
// the proxy that sets them is explicitly configured as trusted.
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("auth-login", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(5),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy("auth-register", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromHours(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy("auth-recovery", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromHours(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy("auth-refresh", context =>
        RateLimitPartition.GetFixedWindowLimiter(
            context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(10),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

});

builder.Services.AddOpenApi();

builder.Services.AddSwaggerGen(options =>
{
    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description =
                "Enter your JWT token. Example: Bearer {your-token}"
        });

    options.AddSecurityRequirement(document =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference(
                "Bearer",
                document)] = []
        });
});

// =========================================================
// CORS
// =========================================================

var allowedFrontendOrigins =
    builder.Configuration
        .GetSection("Cors:AllowedOrigins")
        .Get<string[]>();

if (!builder.Environment.IsDevelopment() &&
    !builder.Environment.IsEnvironment("Testing"))
{
    if (allowedFrontendOrigins == null || allowedFrontendOrigins.Length == 0)
    {
        throw new InvalidOperationException(
            "Cors:AllowedOrigins must list the exact frontend origins in staging and production.");
    }

    foreach (var origin in allowedFrontendOrigins)
    {
        if (!Uri.TryCreate(origin, UriKind.Absolute, out var parsedOrigin) ||
            parsedOrigin.Scheme != Uri.UriSchemeHttps ||
            parsedOrigin.AbsolutePath != "/" ||
            !string.IsNullOrEmpty(parsedOrigin.Query) ||
            !string.IsNullOrEmpty(parsedOrigin.Fragment) ||
            !string.IsNullOrEmpty(parsedOrigin.UserInfo))
        {
            throw new InvalidOperationException(
                $"Cors:AllowedOrigins entry '{origin}' must be an HTTPS origin without a path, query, or credentials.");
        }
    }
}

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        var allowedOrigins = allowedFrontendOrigins;

        if (allowedOrigins != null &&
            allowedOrigins.Length > 0)
        {
            policy
                .WithOrigins(allowedOrigins)
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        }
        else if (builder.Environment.IsDevelopment())
        {
            policy
                .WithOrigins(
                    "http://localhost:3000",
                    "https://rasheepharma.vercel.app")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
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
    // SQL Server is used for local development, staging, and production.
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

        options.Events = new JwtBearerEvents
        {

            // Access tokens are accepted only in explicit Authorization headers; cookies are refresh-only.
            OnTokenValidated = async context =>
            {
                var jti = context.Principal?
                    .FindFirst(
                        System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Jti)
                    ?.Value;

                if (string.IsNullOrWhiteSpace(jti))
                {
                    context.Fail("JWT ID is missing.");
                    return;
                }

                var revocationService = context.HttpContext
                    .RequestServices
                    .GetRequiredService<AccessTokenRevocationService>();

                var isRevoked =
                    await revocationService.IsRevokedAsync(
                        jti,
                        context.HttpContext.RequestAborted);

                if (isRevoked)
                {
                    context.Fail(
                        "Access token has been revoked.");
                }
            }
        };
    });


// =========================================================
// Authorization
// =========================================================

builder.Services.AddAuthorization();

//=======================================================
// Dependency Injection for RefreshTokenRepository
//======================================================

builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
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
    IPasswordResetTokenRepository,
    PasswordResetTokenRepository>();

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

// Blog persistence is registered independently so existing website-content behavior stays unchanged.
builder.Services.AddScoped<IBlogPostRepository, BlogPostRepository>();

builder.Services.AddScoped<
    IBulkUploadRepository,
    BulkUploadRepository>();
builder.Services.AddScoped<IBulkUploadDispatchOutboxRepository, BulkUploadDispatchOutboxRepository>();
builder.Services.AddSingleton<IBulkUploadDispatchSignal, BulkUploadDispatchSignal>();
builder.Services.AddHostedService<LocalBulkUploadStorageCleanupService>();

builder.Services.AddScoped<
    IBulkUploadService,
    BulkUploadService>();


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

builder.Services.AddSingleton(new LocalFileStorageRoot(
    builder.Configuration["Storage:RootPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads")));

builder.Services.AddScoped<IImageStorageService>(sp => new LocalFileImageStorageService(
    sp.GetRequiredService<LocalFileStorageRoot>(), "products"));

builder.Services.AddScoped<IBlogImageStorageService, LocalFileBlogImageStorageService>();

builder.Services.AddScoped<IBrochureStorageService>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    return new LocalFileBrochureStorageService(
        sp.GetRequiredService<LocalFileStorageRoot>(),
        configuration["Email:QuotationBrochure:FileName"] ?? "Rashe_Lifesciences_Client_Brochure_v2.pdf",
        configuration.GetValue<long?>("Email:QuotationBrochure:MaxBytes") ?? 10 * 1024 * 1024);
});

builder.Services.AddScoped<IBulkUploadStorageService, LocalFileBulkUploadStorageService>();

builder.Services.AddScoped<
    IBulkUploadExcelParser,
    ClosedXmlBulkUploadExcelParser>();



builder.Services.AddScoped<
    ICategoryService,
    CategoryService>();

builder.Services.AddScoped<
    IWebsiteContentService,
    WebsiteContentService>();

builder.Services.AddScoped<IBlogPostService, BlogPostService>();

builder.Services.AddScoped<
    IAuthService,
    AuthService>();

builder.Services.AddScoped<
    AccessTokenRevocationService>();

builder.Services.AddScoped<
    IEmailService,
    EmailService>();

builder.Services.AddScoped<
    IInvoiceService,
    RashePharma.Infrastructure.Services.InvoiceService>();

builder.Services.AddScoped<
    IEmailNotificationRepository,
    EmailNotificationRepository>();

builder.Services.AddScoped<
    IEmailNotificationService,
    EmailNotificationService>();

// Enquiry, quotation, and invoice notifications are processed on demand by the API.
// Bulk-upload processing is hosted by the dedicated RabbitMQ worker container.



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

builder.Services.AddScoped<
    IStripeWebhookEventRepository,
    StripeWebhookEventRepository>();


if (!builder.Environment.IsEnvironment("Testing"))
{
    // RabbitMQ transports messages while SQL remains the durable outbox.
    builder.Services.AddRabbitMqTransport(builder.Configuration);
    builder.Services.AddBulkUploadDispatchers(builder.Configuration);
}


// =========================================================
// Build Application
// =========================================================

var app = builder.Build();

var forwardedHeadersOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    ForwardLimit = 1
};
forwardedHeadersOptions.KnownProxies.Clear();
forwardedHeadersOptions.KnownNetworks.Clear();
var trustedProxyAddresses = builder.Configuration
    .GetSection("ForwardedHeaders:KnownProxies")
    .Get<string[]>() ?? [];
if (!builder.Environment.IsDevelopment() && trustedProxyAddresses.Length == 0)
{
    throw new InvalidOperationException(
        "ForwardedHeaders:KnownProxies must list the trusted Nginx proxy address in staging and production.");
}
foreach (var proxyAddress in trustedProxyAddresses)
{
    if (!IPAddress.TryParse(proxyAddress, out var parsedAddress))
        throw new InvalidOperationException($"Forwarded proxy address '{proxyAddress}' is invalid.");
    forwardedHeadersOptions.KnownProxies.Add(parsedAddress);
}
if (builder.Environment.IsDevelopment())
{
    forwardedHeadersOptions.KnownProxies.Add(IPAddress.Loopback);
    forwardedHeadersOptions.KnownProxies.Add(IPAddress.IPv6Loopback);
}





// Database schema changes and bootstrap seeding are executed by
// RashePharma.DbMigrator during the release process, never by API startup.
// This keeps API restarts independent from schema migration locks and DDL work.

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

app.UseForwardedHeaders(forwardedHeadersOptions);
app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.UseCors("Frontend");

app.UseRateLimiter();

app.UseAuthentication();

app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }))
    .AllowAnonymous();

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
