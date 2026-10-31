
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Application.Services;
using RashePharma.Domain.Entities;
using RashePharma.Domain.Enums;
using Xunit;

namespace RashePharma.Api.Tests;

public class BulkUploadServiceFailureHandlingTests
{
    private const string ClaimToken = "failure-test-token";

    private readonly FakeBulkUploadRepository _bulkRepository = new();
    private readonly FakeBulkUploadStorageService _storage = new();
    private readonly FakeExcelParser _parser = new();

    private BulkUploadService CreateService()
    {
        return new BulkUploadService(
            _bulkRepository,
            new FakeProductRepository(),
            new FakeProductVariantRepository(),
            new FakeProductImageRepository(),
            new FakeCategoryRepository(),
            new FakeUnitOfWork(),
            _storage,
            new FakeImageStorageService(),
            _parser,
            new FakeExchangeRateService(),
            NullLogger<BulkUploadService>.Instance);
    }

    private BulkUploadJob CreateJob(
        bool includeExcel = true,
        BulkUploadStatus status = BulkUploadStatus.Processing)
    {
        var job = new BulkUploadJob
        {
            Id = 1,
            FileName = "products.xlsx",
            Status = status,
            ClaimToken = ClaimToken
        };

        if (includeExcel)
        {
            job.Files.Add(new BulkUploadFile
            {
                BulkUploadJobId = 1,
                OriginalFileName = "products.xlsx",
                BlobName = "products-blob",
                FileUrl = "https://storage.test/products.xlsx",
                FileType = "Excel",
                ContentType =
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                FileSize = 1024
            });
        }

        _bulkRepository.Job = job;
        return job;
    }

    // TEST 1: Job does not exist.

    [Fact]
    public async Task ProcessAsync_JobNotFound_ShouldThrowKeyNotFoundException()
    {
        _bulkRepository.Job = null;

        var exception = await Assert.ThrowsAsync<KeyNotFoundException>(
            () => CreateService().ProcessAsync(1, ClaimToken));

        Assert.Contains("Bulk upload job with ID 1 was not found",
            exception.Message);

        Assert.Equal(0, _bulkRepository.UpdateCallCount);
        Assert.Equal(0, _bulkRepository.ClaimCheckCount);
    }

    // TEST 2: Worker has already lost ownership before processing.

    [Fact]
    public async Task ProcessAsync_ClaimOwnershipLost_ShouldThrowOperationCanceledException()
    {
        CreateJob();
        _bulkRepository.ClaimOwner = false;

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => CreateService().ProcessAsync(1, ClaimToken));

        Assert.Equal(0, _bulkRepository.UpdateCallCount);
        Assert.Equal(0, _parser.ParseCallCount);
    }

    // TEST 3: Worker loses ownership while saving progress.

    [Fact]
    public async Task ProcessAsync_ClaimLostDuringProgress_ShouldStopProcessing()
    {
        var job = CreateJob();

        // First progress update fails because ownership was lost.
        _bulkRepository.ReturnFalseOnUpdateCall = 1;

        await Assert.ThrowsAsync<OperationCanceledException>(
            () => CreateService().ProcessAsync(1, ClaimToken));

        Assert.Equal(1, _bulkRepository.UpdateCallCount);
        Assert.Equal(0, _parser.ParseCallCount);

        // Operation cancellation must not mark the job as Failed.
        Assert.Equal(BulkUploadStatus.Processing, job.Status);
    }

    // TEST 4: Excel file is missing from the job.

    [Fact]
    public async Task ProcessAsync_MissingExcelFile_ShouldMarkJobFailed()
    {
        var job = CreateJob(includeExcel: false);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().ProcessAsync(1, ClaimToken));

        Assert.Contains("Excel file was not found", exception.Message);

        Assert.Equal(BulkUploadStatus.Failed, job.Status);
        Assert.Contains("Excel file was not found", job.ErrorMessage);
        Assert.NotNull(job.CompletedAt);

        Assert.Equal(2, _bulkRepository.UpdateCallCount);
        Assert.Contains(
            BulkUploadStatus.Failed,
            _bulkRepository.UpdatedStatuses);
    }

    // TEST 5: Staged Excel download fails.

    [Fact]
    public async Task ProcessAsync_ExcelDownloadException_ShouldMarkJobFailed()
    {
        var job = CreateJob();

        _storage.DownloadException =
            new IOException("Excel storage unavailable");

        var exception = await Assert.ThrowsAsync<IOException>(
            () => CreateService().ProcessAsync(1, ClaimToken));

        Assert.Contains("Excel storage unavailable", exception.Message);

        Assert.Equal(BulkUploadStatus.Failed, job.Status);
        Assert.Contains("Excel storage unavailable", job.ErrorMessage);
        Assert.NotNull(job.CompletedAt);

        Assert.Equal(1, _storage.DownloadCallCount);
        Assert.Equal(2, _bulkRepository.UpdateCallCount);
    }

    // TEST 6: Excel parser throws an exception.

    [Fact]
    public async Task ProcessAsync_ExcelParserException_ShouldMarkJobFailed()
    {
        var job = CreateJob();

        _parser.ParseException =
            new InvalidOperationException("Excel parsing failed");

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().ProcessAsync(1, ClaimToken));

        Assert.Contains("Excel parsing failed", exception.Message);

        Assert.Equal(1, _parser.ParseCallCount);
        Assert.Equal(BulkUploadStatus.Failed, job.Status);
        Assert.Contains("Excel parsing failed", job.ErrorMessage);
        Assert.NotNull(job.CompletedAt);

        Assert.Contains(
            BulkUploadStatus.Failed,
            _bulkRepository.UpdatedStatuses);
    }

    // TEST 7: Failure status update is rejected by repository.

    [Fact]
    public async Task ProcessAsync_FailureStatusUpdateRejected_ShouldPreserveOriginalException()
    {
        var job = CreateJob();

        var originalException =
            new InvalidOperationException("Original parser failure");

        _parser.ParseException = originalException;

        // First update saves Processing.
        // Second update attempts to save Failed and returns false.
        _bulkRepository.ReturnFalseOnUpdateCall = 2;

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().ProcessAsync(1, ClaimToken));

        Assert.Same(originalException, exception);
        Assert.Contains("Original parser failure", exception.Message);

        Assert.Equal(BulkUploadStatus.Failed, job.Status);
        Assert.Equal(2, _bulkRepository.UpdateCallCount);
    }

    // TEST 8: Failure status persistence itself throws an exception.

    [Fact]
    public async Task ProcessAsync_FailureStatusSaveThrows_ShouldPreserveOriginalException()
    {
        var job = CreateJob();

        var originalException =
            new InvalidOperationException("Original Excel failure");

        _parser.ParseException = originalException;

        // The second update is the failure-status persistence attempt.
        _bulkRepository.ThrowOnUpdateCall = 2;

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().ProcessAsync(1, ClaimToken));

        Assert.Same(originalException, exception);
        Assert.Contains("Original Excel failure", exception.Message);

        Assert.Equal(BulkUploadStatus.Failed, job.Status);
        Assert.Equal(2, _bulkRepository.UpdateCallCount);
    }

    // TEST 9: Already completed job should not be processed again.

    [Fact]
    public async Task ProcessAsync_AlreadyCompletedJob_ShouldSkipProcessing()
    {
        var job = CreateJob(
            includeExcel: false,
            status: BulkUploadStatus.Completed);

        await CreateService().ProcessAsync(1, ClaimToken);

        Assert.Equal(BulkUploadStatus.Completed, job.Status);
        Assert.Equal(0, _bulkRepository.UpdateCallCount);
        Assert.Equal(0, _storage.DownloadCallCount);
        Assert.Equal(0, _parser.ParseCallCount);
    }

    // TEST 10: CompletedWithErrors job should not be processed again.

    [Fact]
    public async Task ProcessAsync_CompletedWithErrorsJob_ShouldSkipProcessing()
    {
        var job = CreateJob(
            includeExcel: false,
            status: BulkUploadStatus.CompletedWithErrors);

        await CreateService().ProcessAsync(1, ClaimToken);

        Assert.Equal(BulkUploadStatus.CompletedWithErrors, job.Status);
        Assert.Equal(0, _bulkRepository.UpdateCallCount);
        Assert.Equal(0, _storage.DownloadCallCount);
        Assert.Equal(0, _parser.ParseCallCount);
    }

    // FAKE BULK UPLOAD REPOSITORY

    private sealed class FakeBulkUploadRepository : IBulkUploadRepository
    {
        public BulkUploadJob? Job { get; set; }

        public bool ClaimOwner { get; set; } = true;

        public int ClaimCheckCount { get; private set; }

        public int UpdateCallCount { get; private set; }

        public int? ReturnFalseOnUpdateCall { get; set; }

        public int? ThrowOnUpdateCall { get; set; }

        public List<BulkUploadStatus> UpdatedStatuses { get; } = new();

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
            => Task.FromResult<BulkUploadJob?>(
                Job != null && Job.Id == jobId ? Job : null);

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
        {
            ClaimCheckCount++;

            return Task.FromResult(
                ClaimOwner &&
                Job != null &&
                Job.Id == jobId &&
                Job.ClaimToken == claimToken);
        }

        public Task<bool> UpdateJobIfOwnedAsync(
            BulkUploadJob job,
            string claimToken,
            CancellationToken cancellationToken = default)
        {
            UpdateCallCount++;
            UpdatedStatuses.Add(job.Status);

            if (ThrowOnUpdateCall == UpdateCallCount)
            {
                throw new IOException("Failure status database update failed");
            }

            return Task.FromResult(
                ReturnFalseOnUpdateCall != UpdateCallCount &&
                Job != null &&
                Job.Id == job.Id &&
                Job.ClaimToken == claimToken);
        }

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

    // FAKE STORAGE SERVICE

    private sealed class FakeBulkUploadStorageService
        : IBulkUploadStorageService
    {
        public int DownloadCallCount { get; private set; }

        public Exception? DownloadException { get; set; }

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
        {
            DownloadCallCount++;

            if (DownloadException != null)
            {
                throw DownloadException;
            }

            return Task.FromResult<(Stream, string ContentType)>(
                (new MemoryStream(new byte[] { 1, 2, 3 }),
                 "application/octet-stream"));
        }

        public Task DeleteAsync(string fileUrl)
            => Task.CompletedTask;
    }

    // FAKE EXCEL PARSER

    private sealed class FakeExcelParser : IBulkUploadExcelParser
    {
        public int ParseCallCount { get; private set; }

        public Exception? ParseException { get; set; }

        public BulkUploadExcelData Data { get; set; } = new();

        public Task<BulkUploadExcelData> ParseAsync(
            Stream excelStream,
            CancellationToken cancellationToken = default)
        {
            ParseCallCount++;

            if (ParseException != null)
            {
                throw ParseException;
            }

            return Task.FromResult(Data);
        }
    }

    // FAKE PRODUCT REPOSITORY

    private sealed class FakeProductRepository : IProductRepository
    {
        public Task<List<Product>> GetAllAsync()
            => Task.FromResult(new List<Product>());

        public Task<List<Product>> GetFeaturedAsync()
            => Task.FromResult(new List<Product>());

        public Task<Product?> GetByIdAsync(int id)
            => Task.FromResult<Product?>(null);

        public Task<Product?> GetBySlugAsync(string slug)
            => Task.FromResult<Product?>(null);

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

    // FAKE CATEGORY REPOSITORY

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

    // FAKE VARIANT REPOSITORY

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

    // FAKE IMAGE REPOSITORY

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

    // FAKE IMAGE STORAGE

    private sealed class FakeImageStorageService : IImageStorageService
    {
        public Task<string> UploadAsync(
            Stream fileStream,
            string fileName,
            string? contentType)
            => Task.FromResult("https://images.test/test.png");

        public Task<(Stream Stream, string ContentType)> DownloadAsync(
            string imageUrl)
            => Task.FromResult<(Stream, string ContentType)>(
                (new MemoryStream(),
                 "application/octet-stream"));
    }

    // FAKE EXCHANGE RATE SERVICE

    private sealed class FakeExchangeRateService : IExchangeRateService
    {
        public Task<decimal> GetInrToUsdRateAsync()
            => Task.FromResult(0.012m);
    }

    // FAKE UNIT OF WORK

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync()
            => Task.FromResult(1);

        public Task<ITransaction> BeginTransactionAsync()
            => throw new NotImplementedException();
    }
}
