using Microsoft.EntityFrameworkCore;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Application.Services;
using RashePharma.Infrastructure;
using RashePharma.Infrastructure.Data;
using RashePharma.Infrastructure.Repositories;
using RashePharma.Infrastructure.Services;

namespace RashePharma.Infrastructure.Workers;

public static class BulkUploadWorkerServiceCollectionExtensions
{
    public static IServiceCollection AddBulkUploadWorkerServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is required.");

        services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(connectionString));
        services.AddSingleton(new LocalFileStorageRoot(
            configuration["Storage:RootPath"] ?? Path.Combine(Directory.GetCurrentDirectory(), "uploads")));
        services.AddRabbitMqTransport(configuration);
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<IBulkUploadRepository, BulkUploadRepository>();
        services.AddScoped<IBulkUploadDispatchOutboxRepository, BulkUploadDispatchOutboxRepository>();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IProductVariantRepository, ProductVariantRepository>();
        services.AddScoped<IProductImageRepository, ProductImageRepository>();
        services.AddScoped<IBulkUploadService, BulkUploadService>();
        services.AddScoped<IImageStorageService>(sp => new LocalFileImageStorageService(
            sp.GetRequiredService<LocalFileStorageRoot>(), "products"));
        services.AddScoped<IBulkUploadStorageService, LocalFileBulkUploadStorageService>();
        services.AddScoped<IBulkUploadExcelParser, ClosedXmlBulkUploadExcelParser>();
        services.AddHttpClient<IExchangeRateService, ExchangeRateService>();
        services.AddHostedService<BulkUploadRabbitMqConsumer>();

        return services;
    }
}
