using Microsoft.EntityFrameworkCore;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Application.Services;
using RashePharma.Infrastructure;
using RashePharma.Infrastructure.Data;
using RashePharma.Infrastructure.Repositories;
using RashePharma.Infrastructure.Services;
using RashePharma.Infrastructure.Workers;

var builder = Host.CreateApplicationBuilder(args);
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");
var storageConnection = builder.Configuration.GetConnectionString("AzureStorage")
    ?? throw new InvalidOperationException("ConnectionStrings:AzureStorage is required.");

builder.Services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddAzureServiceBus(builder.Configuration);
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IBulkUploadRepository, BulkUploadRepository>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
builder.Services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
builder.Services.AddScoped<IProductImageRepository, ProductImageRepository>();
builder.Services.AddScoped<IBulkUploadService, BulkUploadService>();
builder.Services.AddScoped<IImageStorageService>(_ => new AzureBlobImageStorageService(storageConnection));
builder.Services.AddScoped<IBulkUploadStorageService>(_ => new AzureBlobBulkUploadStorageService(storageConnection));
builder.Services.AddScoped<IBulkUploadExcelParser, ClosedXmlBulkUploadExcelParser>();
builder.Services.AddHttpClient<IExchangeRateService, ExchangeRateService>();
builder.Services.AddHostedService<BulkUploadServiceBusConsumer>();

await builder.Build().RunAsync();
