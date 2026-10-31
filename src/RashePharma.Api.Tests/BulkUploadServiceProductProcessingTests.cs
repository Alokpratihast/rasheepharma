using Microsoft.Extensions.Logging.Abstractions;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Application.Services;
using RashePharma.Domain.Entities;
using RashePharma.Domain.Enums;
using Xunit;




namespace RashePharma.Api.Tests;

public class BulkUploadServiceProductProcessingTests
{
    private const string ClaimToken = "test-claim-token";

    private readonly FakeBulkUploadRepository _bulkRepository = new();
    private readonly FakeProductRepository _productRepository = new();
    private readonly FakeCategoryRepository _categoryRepository = new();
    private readonly FakeProductVariantRepository _variantRepository = new();
    private readonly FakeProductImageRepository _imageRepository = new();
    private readonly FakeBulkUploadStorageService _storage = new();
    private readonly FakeExcelParser _parser = new();
    private readonly FakeExchangeRateService _exchangeRate = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private BulkUploadService CreateService()
    {
        return new BulkUploadService(
            _bulkRepository,
            _productRepository,
            _variantRepository,
            _imageRepository,
            _categoryRepository,
            _unitOfWork,
            _storage,
            new FakeImageStorageService(),
            _parser,
            _exchangeRate,
            NullLogger<BulkUploadService>.Instance);
    }

    private BulkUploadJob CreateJob()
    {
        var job = new BulkUploadJob
        {
            Id = 1,
            FileName = "products.xlsx",
            Status = BulkUploadStatus.Processing,
            ClaimToken = ClaimToken,
            Files = new List<BulkUploadFile>
            {
                new()
                {
                    BulkUploadJobId = 1,
                    OriginalFileName = "products.xlsx",
                    BlobName = "products-blob",
                    FileUrl = "https://storage.test/products.xlsx",
                    FileType = "Excel",
                    ContentType =
                        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    FileSize = 1024
                }
            }
        };

        _bulkRepository.Job = job;
        return job;
    }

    private async Task ProcessAsync(BulkUploadProductRow productRow)
    {
        _parser.Data = new BulkUploadExcelData
        {
            Products = new List<BulkUploadProductRow> { productRow }
        };

        CreateJob();

        await CreateService().ProcessAsync(1, ClaimToken);
    }

    private async Task ProcessVariantAsync(BulkUploadVariantRow variantRow)
{
    _parser.Data = new BulkUploadExcelData
    {
        Products = new List<BulkUploadProductRow>(),
        Variants = new List<BulkUploadVariantRow> { variantRow }
    };

    CreateJob();

    await CreateService().ProcessAsync(1, ClaimToken);
}

private static BulkUploadVariantRow ValidVariant()
{
    return new BulkUploadVariantRow
    {
        ProductSlug = "test-medicine",
        Strength = "500mg",
        PackSize = "10 Tablets",
        PriceInr = 1000,
        MOQ = 5,
        UnitType = "Tablet",
        SKU = "TEST-500",
        StockQuantity = 100,
        IsActive = true
    };
}

[Fact]
public async Task ProcessAsync_NewVariant_ShouldCreateVariant()
{
    _productRepository.ExistingProduct = new Product
    {
        Id = 10,
        Name = "Test Medicine",
        Slug = "test-medicine"
    };

    await ProcessVariantAsync(ValidVariant());

    var variant = Assert.Single(_variantRepository.AddedVariants);

    Assert.Equal(10, variant.ProductId);
    Assert.Equal("500mg", variant.Strength);
    Assert.Equal("TEST-500", variant.SKU);
    Assert.Equal(12.00m, variant.Price);
    Assert.Equal("USD", variant.Currency);

    Assert.Equal(1, _bulkRepository.Job!.SuccessCount);
    Assert.Equal(0, _bulkRepository.Job.ErrorCount);
}

[Fact]
public async Task ProcessAsync_ExistingVariantWithSameSku_ShouldUpdateVariant()
{
    _productRepository.ExistingProduct = new Product
    {
        Id = 10,
        Slug = "test-medicine"
    };

    var existing = new ProductVariant
    {
        Id = 20,
        ProductId = 10,
        SKU = "TEST-500",
        Strength = "250mg",
        Price = 5
    };

    _variantRepository.ExistingVariants.Add(existing);

    var row = ValidVariant();
    row.PriceInr = 2000;

    await ProcessVariantAsync(row);

    Assert.Equal(0, _variantRepository.AddCallCount);
    Assert.Equal(1, _variantRepository.UpdateCallCount);

    Assert.Equal("500mg", existing.Strength);
    Assert.Equal(24.00m, existing.Price);
    Assert.Equal("USD", existing.Currency);
    Assert.Equal(24.00m, existing.Price);
}

[Fact]
public async Task ProcessAsync_ExistingVariantWithSameAttributes_ShouldUpdateVariant()
{
    _productRepository.ExistingProduct = new Product
    {
        Id = 10,
        Slug = "test-medicine"
    };

    var existing = new ProductVariant
    {
        Id = 21,
        ProductId = 10,
        Strength = "500mg",
        PackSize = "10 Tablets",
        UnitType = "Tablet",
        SKU = "OLD-SKU"
    };

    _variantRepository.ExistingVariants.Add(existing);

    var row = ValidVariant();
    row.SKU = "NEW-SKU";

    await ProcessVariantAsync(row);

    Assert.Equal(0, _variantRepository.AddCallCount);
    Assert.Equal(1, _variantRepository.UpdateCallCount);
    Assert.Equal("NEW-SKU", existing.SKU);
}

[Theory]
[InlineData(0, 5, 100)]
[InlineData(-100, 5, 100)]
public async Task ProcessAsync_InvalidPrice_ShouldRecordError(
    decimal price, int moq, int stock)
{
    _productRepository.ExistingProduct = new Product
    {
        Id = 10,
        Slug = "test-medicine"
    };

    var row = ValidVariant();
    row.PriceInr = price;
    row.MOQ = moq;
    row.StockQuantity = stock;

    await ProcessVariantAsync(row);

    Assert.Equal(0, _variantRepository.AddCallCount);
    Assert.Equal(1, _bulkRepository.Job!.ErrorCount);
    Assert.Contains(
        "Price INR must be greater than zero",
        Assert.Single(_bulkRepository.Job.Errors).ErrorMessage);
}

[Fact]
public async Task ProcessAsync_NegativeMOQ_ShouldRecordError()
{
    _productRepository.ExistingProduct = new Product
    {
        Id = 10,
        Slug = "test-medicine"
    };

    var row = ValidVariant();
    row.MOQ = -1;

    await ProcessVariantAsync(row);

    Assert.Equal(0, _variantRepository.AddCallCount);
    Assert.Equal(1, _bulkRepository.Job!.ErrorCount);
    Assert.Contains(
        "MOQ cannot be negative",
        Assert.Single(_bulkRepository.Job.Errors).ErrorMessage);
}

[Fact]
public async Task ProcessAsync_NegativeStock_ShouldRecordError()
{
    _productRepository.ExistingProduct = new Product
    {
        Id = 10,
        Slug = "test-medicine"
    };

    var row = ValidVariant();
    row.StockQuantity = -1;

    await ProcessVariantAsync(row);

    Assert.Equal(0, _variantRepository.AddCallCount);
    Assert.Equal(1, _bulkRepository.Job!.ErrorCount);
    Assert.Contains(
        "Stock quantity cannot be negative",
        Assert.Single(_bulkRepository.Job.Errors).ErrorMessage);
}

[Fact]
public async Task ProcessAsync_ProductNotFound_ShouldRecordError()
{
    var row = ValidVariant();

    await ProcessVariantAsync(row);

    Assert.Equal(0, _variantRepository.AddCallCount);
    Assert.Equal(1, _bulkRepository.Job!.ErrorCount);
    Assert.Contains(
        "Product 'test-medicine' was not found",
        Assert.Single(_bulkRepository.Job.Errors).ErrorMessage);
}

[Fact]
public async Task ProcessAsync_SkuMatching_ShouldIgnoreCase()
{
    _productRepository.ExistingProduct = new Product
    {
        Id = 10,
        Slug = "test-medicine"
    };

    var existing = new ProductVariant
    {
        Id = 22,
        ProductId = 10,
        SKU = "test-500"
    };

    _variantRepository.ExistingVariants.Add(existing);

    var row = ValidVariant();
    row.SKU = "TEST-500";

    await ProcessVariantAsync(row);

    Assert.Equal(0, _variantRepository.AddCallCount);
    Assert.Equal(1, _variantRepository.UpdateCallCount);
}

    private static BulkUploadProductRow ValidProduct()
    {
        return new BulkUploadProductRow
        {
            Category = "Antibiotics",
            ProductName = "Test Medicine",
            Slug = "test-medicine",
            GenericName = "Test Generic",
            Composition = "Test Composition",
            DosageForm = "Tablet",
            Description = "Test description",
            BrandName = "Test Brand",
            Manufacturer = "Test Manufacturer",
            IsActive = true,
            IsFeatured = false
        };
    }

    [Fact]
    public async Task ProcessAsync_ValidProduct_ShouldCreateNewProduct()
    {
        await ProcessAsync(ValidProduct());

        Assert.Equal(1, _categoryRepository.AddCallCount);
        Assert.Equal(1, _productRepository.AddCallCount);
        Assert.Equal(0, _productRepository.UpdateCallCount);

        var product = Assert.Single(_productRepository.AddedProducts);

        Assert.Equal("Test Medicine", product.Name);
        Assert.Equal("test-medicine", product.Slug);
        Assert.Equal("Test Generic", product.GenericName);
        Assert.Equal("Antibiotics", _categoryRepository.AddedCategories[0].Name);

        Assert.Equal(1, _bulkRepository.Job!.SuccessCount);
        Assert.Equal(0, _bulkRepository.Job.ErrorCount);
        Assert.Equal(BulkUploadStatus.Completed, _bulkRepository.Job.Status);
    }

    [Fact]
    public async Task ProcessAsync_ExistingProduct_ShouldUpdateProduct()
    {
        var existingProduct = new Product
        {
            Id = 10,
            Name = "Old Medicine",
            Slug = "test-medicine",
            CategoryId = 5,
            IsActive = false
        };

        _productRepository.ExistingProduct = existingProduct;

        await ProcessAsync(ValidProduct());

        Assert.Equal(0, _productRepository.AddCallCount);
        Assert.Equal(1, _productRepository.UpdateCallCount);

        Assert.Equal("Test Medicine", existingProduct.Name);
        Assert.Equal("Test Generic", existingProduct.GenericName);
        Assert.Equal("Test Composition", existingProduct.Composition);
        Assert.Equal("Test Manufacturer", existingProduct.Manufacturer);
        Assert.True(existingProduct.IsActive);

        Assert.Equal(1, _bulkRepository.Job!.SuccessCount);
        Assert.Equal(0, _bulkRepository.Job.ErrorCount);
        Assert.Equal(BulkUploadStatus.Completed, _bulkRepository.Job.Status);
    }

    [Fact]
    public async Task ProcessAsync_MissingProductName_ShouldRecordError()
    {
        var row = ValidProduct();
        row.ProductName = "";

        await ProcessAsync(row);

        Assert.Equal(0, _productRepository.AddCallCount);
        Assert.Equal(0, _categoryRepository.AddCallCount);

        Assert.Equal(1, _bulkRepository.Job!.ErrorCount);
        Assert.Equal(0, _bulkRepository.Job.SuccessCount);
        Assert.Equal(BulkUploadStatus.CompletedWithErrors, _bulkRepository.Job.Status);

        var error = Assert.Single(_bulkRepository.Job.Errors);
        Assert.Equal("Products", error.SheetName);
        Assert.Equal(2, error.RowNumber);
        Assert.Equal("test-medicine", error.ProductSlug);
        Assert.Contains("Product name is required", error.ErrorMessage);
    }

    [Fact]
    public async Task ProcessAsync_MissingCategory_ShouldRecordError()
    {
        var row = ValidProduct();
        row.Category = "";

        await ProcessAsync(row);

        Assert.Equal(0, _categoryRepository.AddCallCount);
        Assert.Equal(0, _productRepository.AddCallCount);

        Assert.Equal(1, _bulkRepository.Job!.ErrorCount);
        Assert.Equal(0, _bulkRepository.Job.SuccessCount);
        Assert.Equal(BulkUploadStatus.CompletedWithErrors, _bulkRepository.Job.Status);

        var error = Assert.Single(_bulkRepository.Job.Errors);
        Assert.Equal("Products", error.SheetName);
        Assert.Contains("Category is required", error.ErrorMessage);
    }

    [Fact]
    public async Task ProcessAsync_ProductRepositoryException_ShouldRecordRowError()
    {
        _productRepository.AddException =
            new InvalidOperationException("Database insert failed");

        await ProcessAsync(ValidProduct());

        Assert.Equal(1, _productRepository.AddCallCount);
        Assert.Equal(0, _productRepository.UpdateCallCount);

        Assert.Equal(1, _bulkRepository.Job!.ErrorCount);
        Assert.Equal(0, _bulkRepository.Job.SuccessCount);
        Assert.Equal(BulkUploadStatus.CompletedWithErrors, _bulkRepository.Job.Status);

        var error = Assert.Single(_bulkRepository.Job.Errors);
        Assert.Equal("Products", error.SheetName);
        Assert.Contains("Database insert failed", error.ErrorMessage);
    }

    private sealed class FakeBulkUploadRepository : IBulkUploadRepository
    {
        public BulkUploadJob? Job { get; set; }

        public Task<BulkUploadJob> CreateJobAsync(
            BulkUploadJob job,
            CancellationToken cancellationToken = default)
        {
            Job = job;
            job.Id = 1;
            return Task.FromResult(job);
        }

        public Task<BulkUploadJob?> GetByIdAsync(
            int jobId,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Job);

        public Task<BulkUploadJob?> ClaimNextPendingJobAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult<BulkUploadJob?>(null);

        public Task<bool> TryRenewLeaseAsync(
            int jobId,
            string claimToken,
            TimeSpan leaseDuration,
            CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task<bool> IsClaimOwnerAsync(
            int jobId,
            string claimToken,
            CancellationToken cancellationToken = default)
            => Task.FromResult(
                Job != null &&
                Job.Id == jobId &&
                Job.ClaimToken == claimToken);

        public Task<bool> UpdateJobIfOwnedAsync(
            BulkUploadJob job,
            string claimToken,
            CancellationToken cancellationToken = default)
            => Task.FromResult(
                Job != null &&
                Job.Id == job.Id &&
                Job.ClaimToken == claimToken);

        public Task UpdateJobAsync(
            BulkUploadJob job,
            CancellationToken cancellationToken = default)
        {
            Job = job;
            return Task.CompletedTask;
        }

        public Task AddErrorAsync(
            BulkUploadError error,
            CancellationToken cancellationToken = default)
        {
            Job?.Errors.Add(error);
            return Task.CompletedTask;
        }

        public Task AddFileAsync(
            BulkUploadFile file,
            CancellationToken cancellationToken = default)
        {
            Job?.Files.Add(file);
            return Task.CompletedTask;
        }
    }

    private sealed class FakeProductRepository : IProductRepository
    {
        public Product? ExistingProduct { get; set; }
        public Exception? AddException { get; set; }

        public int AddCallCount { get; private set; }
        public int UpdateCallCount { get; private set; }

        public List<Product> AddedProducts { get; } = new();

        public Task<List<Product>> GetAllAsync()
            => Task.FromResult(new List<Product>());

        public Task<List<Product>> GetFeaturedAsync()
            => Task.FromResult(new List<Product>());

        public Task<Product?> GetByIdAsync(int id)
            => Task.FromResult<Product?>(null);

        public Task<Product?> GetBySlugAsync(string slug)
            => Task.FromResult(
                ExistingProduct != null &&
                ExistingProduct.Slug == slug
                    ? ExistingProduct
                    : null);

        public Task AddAsync(Product product)
        {
            AddCallCount++;

            if (AddException != null)
                throw AddException;

            product.Id = 100;
            AddedProducts.Add(product);

            return Task.CompletedTask;
        }

        public Task UpdateAsync(Product product)
        {
            UpdateCallCount++;
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Product product)
            => Task.CompletedTask;

        public Task<bool> ExistsBySlugAsync(string slug)
            => Task.FromResult(false);

        public Task<int> GetTotalCountAsync()
            => Task.FromResult(0);

        public Task<int> GetFeaturedCountAsync()
            => Task.FromResult(0);
    }

    private sealed class FakeCategoryRepository : ICategoryRepository
    {
        public int AddCallCount { get; private set; }

        public List<Category> AddedCategories { get; } = new();

        public Task<List<Category>> GetAllAsync()
            => Task.FromResult(new List<Category>());

        public Task<List<Category>> GetNavigationAsync()
            => Task.FromResult(new List<Category>());

        public Task<Category?> GetByIdAsync(int id)
            => Task.FromResult<Category?>(null);

        public Task<Category?> GetBySlugAsync(string slug)
            => Task.FromResult<Category?>(
                AddedCategories.FirstOrDefault(c => c.Slug == slug));

        public Task AddAsync(Category category)
        {
            AddCallCount++;
            category.Id = 5;
            AddedCategories.Add(category);
            return Task.CompletedTask;
        }

        public Task UpdateAsync(Category category)
            => Task.CompletedTask;

        public Task DeleteAsync(Category category)
            => Task.CompletedTask;

        public Task<bool> ExistsBySlugAsync(string slug)
            => Task.FromResult(false);

        public Task<int> GetTotalCountAsync()
            => Task.FromResult(0);

        public Task<bool> HasChildrenAsync(int categoryId)
            => Task.FromResult(false);

        public Task<bool> HasProductsAsync(int categoryId)
            => Task.FromResult(false);
    }

   private sealed class FakeProductVariantRepository
    : IProductVariantRepository
{
    public List<ProductVariant> ExistingVariants { get; } = new();

    public List<ProductVariant> AddedVariants { get; } = new();

    public int AddCallCount { get; private set; }

    public int UpdateCallCount { get; private set; }

    public Task<List<ProductVariant>> GetByProductIdAsync(int productId)
        => Task.FromResult(
            ExistingVariants
                .Where(v => v.ProductId == productId)
                .ToList());

    public Task<ProductVariant?> GetByIdAsync(int id)
        => Task.FromResult<ProductVariant?>(
            ExistingVariants.FirstOrDefault(v => v.Id == id));

    public Task AddAsync(ProductVariant variant)
    {
        AddCallCount++;
        variant.Id = 100;
        AddedVariants.Add(variant);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(ProductVariant variant)
    {
        UpdateCallCount++;
        return Task.CompletedTask;
    }

    public Task DeleteAsync(ProductVariant variant)
        => Task.CompletedTask;
}
    private sealed class FakeProductImageRepository
        : IProductImageRepository
    {
        public Task<List<ProductImage>> GetByProductIdAsync(int productId)
            => Task.FromResult(new List<ProductImage>());

        public Task<ProductImage?> GetByIdAsync(int id)
            => Task.FromResult<ProductImage?>(null);

        public Task AddAsync(ProductImage image)
            => Task.CompletedTask;

        public Task UpdateAsync(ProductImage image)
            => Task.CompletedTask;

        public Task DeleteAsync(ProductImage image)
            => Task.CompletedTask;
    }

    private sealed class FakeBulkUploadStorageService
        : IBulkUploadStorageService
    {
        public Task EnsureContainerExistsAsync(
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task<BulkUploadStorageTarget> CreateUploadTargetAsync(
            string fileName,
            CancellationToken cancellationToken = default)
            => throw new NotImplementedException();

        public string GetStagedFileUrl(string blobName)
            => $"https://storage.test/{blobName}";

        public Task<bool> VerifyStagedFileAsync(
            string blobName,
            long expectedFileSize,
            CancellationToken cancellationToken = default)
            => Task.FromResult(true);

        public Task<string> UploadAsync(
            Stream fileStream,
            string fileName,
            string? contentType)
            => throw new NotImplementedException();

        public Task<(Stream Stream, string ContentType)> DownloadAsync(
            string fileUrl)
            => Task.FromResult<(Stream, string ContentType)>(
                (new MemoryStream(new byte[] { 1, 2, 3 }), "application/octet-stream"));

        public Task DeleteAsync(string fileUrl)
            => Task.CompletedTask;
    }

    private sealed class FakeImageStorageService : IImageStorageService
    {
        public Task<string> UploadAsync(
            Stream fileStream,
            string fileName,
            string? contentType)
            => throw new NotImplementedException();

        public Task<(Stream Stream, string ContentType)> DownloadAsync(
            string imageUrl)
            => throw new NotImplementedException();
    }

    private sealed class FakeExcelParser : IBulkUploadExcelParser
    {
        public BulkUploadExcelData Data { get; set; } = new();

        public Task<BulkUploadExcelData> ParseAsync(
            Stream excelStream,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Data);
    }

    private sealed class FakeExchangeRateService : IExchangeRateService
    {
        public Task<decimal> GetInrToUsdRateAsync()
            => Task.FromResult(0.012m);
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync()
            => Task.FromResult(1);

        public Task<ITransaction> BeginTransactionAsync()
            => throw new NotImplementedException();
    }
}
