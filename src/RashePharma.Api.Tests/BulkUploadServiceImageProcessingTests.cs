
using Microsoft.Extensions.Logging.Abstractions;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Application.Services;
using RashePharma.Domain.Entities;
using RashePharma.Domain.Enums;
using Xunit;

namespace RashePharma.Api.Tests;

public class BulkUploadServiceImageProcessingTests
{
    private const string ClaimToken = "image-test-claim-token";

    private readonly FakeBulkUploadRepository _bulkRepository = new();
    private readonly FakeProductRepository _productRepository = new();
    private readonly FakeCategoryRepository _categoryRepository = new();
    private readonly FakeProductVariantRepository _variantRepository = new();
    private readonly FakeProductImageRepository _imageRepository = new();
    private readonly FakeBulkUploadStorageService _bulkStorage = new();
    private readonly FakeImageStorageService _imageStorage = new();
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
            _bulkStorage,
            _imageStorage,
            _parser,
            _exchangeRate,
            NullLogger<BulkUploadService>.Instance);
    }

    private async Task ProcessImageAsync(
        BulkUploadImageRow row,
        bool includeStagedImage = true,
        string stagedFileName = "medicine.png")
    {
        _parser.Data = new BulkUploadExcelData
        {
            Images = new List<BulkUploadImageRow> { row }
        };

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
                    ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    FileSize = 1024
                }
            }
        };

        if (includeStagedImage)
        {
            job.Files.Add(new BulkUploadFile
            {
                BulkUploadJobId = 1,
                OriginalFileName = stagedFileName,
                BlobName = "medicine-image-blob",
                FileUrl = $"https://storage.test/{stagedFileName}",
                FileType = "Image",
                ContentType = "image/png",
                FileSize = 512
            });
        }

        _bulkRepository.Job = job;
        await CreateService().ProcessAsync(1, ClaimToken);
    }

    private static BulkUploadImageRow ValidImage()
    {
        return new BulkUploadImageRow
        {
            ProductSlug = "test-medicine",
            ImageFileName = "medicine.png",
            AltText = "Test medicine image",
            IsPrimary = true,
            DisplayOrder = 1
        };
    }

    private void SetExistingProduct()
    {
        _productRepository.ExistingProduct = new Product
        {
            Id = 10,
            Name = "Test Medicine",
            Slug = "test-medicine"
        };
    }

    // TEST 1: Upload image and create database record.

    [Fact]
    public async Task ProcessAsync_ValidImage_ShouldUploadAndCreateProductImage()
    {
        SetExistingProduct();

        await ProcessImageAsync(ValidImage());

        var image = Assert.Single(_imageRepository.AddedImages);

        Assert.Equal(10, image.ProductId);
        Assert.Equal("https://images.test/medicine.png", image.ImageUrl);
        Assert.Equal("Test medicine image", image.AltText);
        Assert.True(image.IsPrimary);
        Assert.Equal(1, image.DisplayOrder);

        Assert.Equal(1, _imageRepository.AddCallCount);
        Assert.Equal(1, _imageStorage.UploadCallCount);
        Assert.Equal("medicine.png", Assert.Single(_imageStorage.UploadedFileNames));

        Assert.Equal(1, _bulkRepository.Job!.SuccessCount);
        Assert.Equal(0, _bulkRepository.Job.ErrorCount);
        Assert.Equal(BulkUploadStatus.Completed, _bulkRepository.Job.Status);
    }

    // TEST 2: First image should automatically become primary.

    [Fact]
    public async Task ProcessAsync_FirstImage_ShouldBecomePrimaryWhenNoPrimaryExists()
    {
        SetExistingProduct();

        var row = ValidImage();
        row.IsPrimary = false;

        await ProcessImageAsync(row);

        Assert.True(Assert.Single(_imageRepository.AddedImages).IsPrimary);
    }

    // TEST 3: New primary image should unset old primary image.

    [Fact]
    public async Task ProcessAsync_NewPrimaryImage_ShouldUnsetExistingPrimaryImage()
    {
        SetExistingProduct();

        var existingPrimary = new ProductImage
        {
            Id = 30,
            ProductId = 10,
            ImageUrl = "https://images.test/old.png",
            IsPrimary = true
        };

        _imageRepository.ExistingImages.Add(existingPrimary);

        var row = ValidImage();
        row.IsPrimary = true;

        await ProcessImageAsync(row);

        Assert.False(existingPrimary.IsPrimary);
        Assert.True(Assert.Single(_imageRepository.AddedImages).IsPrimary);
    }

    // TEST 4: Non-primary image should not affect existing primary image.

    [Fact]
    public async Task ProcessAsync_NonPrimaryImage_ShouldKeepExistingPrimaryImage()
    {
        SetExistingProduct();

        var existingPrimary = new ProductImage
        {
            Id = 31,
            ProductId = 10,
            ImageUrl = "https://images.test/primary.png",
            IsPrimary = true
        };

        _imageRepository.ExistingImages.Add(existingPrimary);

        var row = ValidImage();
        row.IsPrimary = false;

        await ProcessImageAsync(row);

        Assert.True(existingPrimary.IsPrimary);
        Assert.False(Assert.Single(_imageRepository.AddedImages).IsPrimary);
    }

    // TEST 5: Missing staged image should record an error.

    [Fact]
    public async Task ProcessAsync_MissingStagedImage_ShouldRecordError()
    {
        SetExistingProduct();

        await ProcessImageAsync(
            ValidImage(),
            includeStagedImage: false);

        Assert.Empty(_imageRepository.AddedImages);
        Assert.Equal(0, _imageStorage.UploadCallCount);
        Assert.Equal(1, _bulkRepository.Job!.ErrorCount);

        var error = Assert.Single(_bulkRepository.Job.Errors);

        Assert.Equal("Images", error.SheetName);
        Assert.Equal(2, error.RowNumber);
        Assert.Contains(
            "Image file 'medicine.png' was not uploaded",
            error.ErrorMessage);

        Assert.Equal(
            BulkUploadStatus.CompletedWithErrors,
            _bulkRepository.Job.Status);
    }

    // TEST 6: Missing product should record an error.

    [Fact]
    public async Task ProcessAsync_ProductNotFoundForImage_ShouldRecordError()
    {
        await ProcessImageAsync(ValidImage());

        Assert.Empty(_imageRepository.AddedImages);
        Assert.Equal(0, _imageStorage.UploadCallCount);
        Assert.Equal(1, _bulkRepository.Job!.ErrorCount);

        Assert.Contains(
            "Product 'test-medicine' was not found",
            Assert.Single(_bulkRepository.Job.Errors).ErrorMessage);

        Assert.Equal(
            BulkUploadStatus.CompletedWithErrors,
            _bulkRepository.Job.Status);
    }

    // TEST 7: Missing product slug should record an error.

    [Fact]
    public async Task ProcessAsync_MissingProductSlug_ShouldRecordError()
    {
        SetExistingProduct();

        var row = ValidImage();
        row.ProductSlug = " ";

        await ProcessImageAsync(row);

        Assert.Empty(_imageRepository.AddedImages);
        Assert.Equal(0, _imageStorage.UploadCallCount);
        Assert.Equal(1, _bulkRepository.Job!.ErrorCount);

        Assert.Contains(
            "Product slug is required",
            Assert.Single(_bulkRepository.Job.Errors).ErrorMessage);
    }

    // TEST 8: Image storage exception should record a row error.

    [Fact]
    public async Task ProcessAsync_ImageUploadException_ShouldRecordRowError()
    {
        SetExistingProduct();

        _imageStorage.UploadException =
            new IOException("Image storage unavailable");

        await ProcessImageAsync(ValidImage());

        Assert.Empty(_imageRepository.AddedImages);
        Assert.Equal(1, _imageStorage.UploadCallCount);
        Assert.Equal(1, _bulkRepository.Job!.ErrorCount);

        Assert.Contains(
            "Image storage unavailable",
            Assert.Single(_bulkRepository.Job.Errors).ErrorMessage);

        Assert.Equal(
            BulkUploadStatus.CompletedWithErrors,
            _bulkRepository.Job.Status);
    }

    // TEST 9: Blank image filename should be skipped.

    [Fact]
    public async Task ProcessAsync_BlankImageFileName_ShouldSkipRow()
    {
        var row = ValidImage();
        row.ImageFileName = " ";

        await ProcessImageAsync(row);

        Assert.Empty(_imageRepository.AddedImages);
        Assert.Equal(0, _imageStorage.UploadCallCount);
        Assert.Equal(0, _bulkRepository.Job!.ProcessedRecords);
        Assert.Equal(0, _bulkRepository.Job.SuccessCount);
        Assert.Equal(0, _bulkRepository.Job.ErrorCount);

        Assert.Equal(
            BulkUploadStatus.Completed,
            _bulkRepository.Job.Status);
    }

    // TEST 10: Image filename matching should ignore case.

    [Fact]
    public async Task ProcessAsync_ImageFileNameMatching_ShouldIgnoreCase()
    {
        SetExistingProduct();

        var row = ValidImage();
        row.ImageFileName = "MEDICINE.PNG";

        await ProcessImageAsync(
            row,
            stagedFileName: "medicine.png");

        Assert.Single(_imageRepository.AddedImages);
        Assert.Equal(1, _imageStorage.UploadCallCount);
        Assert.Equal(1, _bulkRepository.Job!.SuccessCount);
    }

    // FAKE REPOSITORIES AND SERVICES

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
            => Task.CompletedTask;

        public Task UpdateAsync(Product product)
            => Task.CompletedTask;

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
        public Task<List<Category>> GetAllAsync()
            => Task.FromResult(new List<Category>());

        public Task<List<Category>> GetNavigationAsync()
            => Task.FromResult(new List<Category>());

        public Task<Category?> GetByIdAsync(int id)
            => Task.FromResult<Category?>(null);

        public Task<Category?> GetBySlugAsync(string slug)
            => Task.FromResult<Category?>(null);

        public Task AddAsync(Category category)
            => Task.CompletedTask;

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
        public Task<List<ProductVariant>> GetByProductIdAsync(int productId)
            => Task.FromResult(new List<ProductVariant>());

        public Task<ProductVariant?> GetByIdAsync(int id)
            => Task.FromResult<ProductVariant?>(null);

        public Task AddAsync(ProductVariant variant)
            => Task.CompletedTask;

        public Task UpdateAsync(ProductVariant variant)
            => Task.CompletedTask;

        public Task DeleteAsync(ProductVariant variant)
            => Task.CompletedTask;
    }

    private sealed class FakeProductImageRepository
        : IProductImageRepository
    {
        public List<ProductImage> ExistingImages { get; } = new();
        public List<ProductImage> AddedImages { get; } = new();

        public int AddCallCount { get; private set; }

        public Task<List<ProductImage>> GetByProductIdAsync(int productId)
            => Task.FromResult(
                ExistingImages
                    .Where(image => image.ProductId == productId)
                    .ToList());

        public Task<ProductImage?> GetByIdAsync(int id)
            => Task.FromResult<ProductImage?>(
                ExistingImages
                    .Concat(AddedImages)
                    .FirstOrDefault(image => image.Id == id));

        public Task AddAsync(ProductImage image)
        {
            AddCallCount++;
            image.Id = 100 + AddedImages.Count;
            AddedImages.Add(image);
            return Task.CompletedTask;
        }

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
                (new MemoryStream(new byte[] { 1, 2, 3 }),
                "application/octet-stream"));

        public Task DeleteAsync(string fileUrl)
            => Task.CompletedTask;
    }

    private sealed class FakeImageStorageService : IImageStorageService
    {
        public int UploadCallCount { get; private set; }

        public List<string> UploadedFileNames { get; } = new();

        public Exception? UploadException { get; set; }

        public Task<string> UploadAsync(
            Stream fileStream,
            string fileName,
            string? contentType)
        {
            UploadCallCount++;

            if (UploadException != null)
            {
                throw UploadException;
            }

            UploadedFileNames.Add(fileName);

            return Task.FromResult(
                $"https://images.test/{fileName}");
        }

        public Task<(Stream Stream, string ContentType)> DownloadAsync(
            string imageUrl)
            => Task.FromResult<(Stream, string ContentType)>(
                (new MemoryStream(new byte[] { 1, 2, 3 }),
                "application/octet-stream"));
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
