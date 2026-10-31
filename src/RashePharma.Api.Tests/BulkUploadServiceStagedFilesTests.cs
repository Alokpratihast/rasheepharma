
using Microsoft.Extensions.Logging.Abstractions;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Application.Services;
using RashePharma.Domain.Entities;
using RashePharma.Domain.Enums;
using Xunit;

namespace RashePharma.Api.Tests;

public class BulkUploadServiceStagedFilesTests
{
    private const string ExcelContentType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private readonly FakeBulkUploadRepository _repository = new();
    private readonly FakeBulkUploadStorageService _storage = new();
    private readonly FakeUnitOfWork _unitOfWork = new();

    private BulkUploadService CreateService()
    {
        return new BulkUploadService(
            _repository,
            null!,
            null!,
            null!,
            null!,
            _unitOfWork,
            _storage,
            null!,
            null!,
            null!,
            NullLogger<BulkUploadService>.Instance);
    }

    [Fact]
    public async Task CreateJobFromStagedFilesAsync_ValidFiles_ShouldCreatePendingJob()
    {
        var files = new List<BulkUploadStagedFileInput>
        {
            new("products.xlsx", ExcelContentType, 1024, "Excel", "excel-blob"),
            new("product.jpg", "image/jpeg", 2048, "Image", "image-blob")
        };

        var jobId = await CreateService()
            .CreateJobFromStagedFilesAsync(files);

        Assert.Equal(1, jobId);
        Assert.NotNull(_repository.CreatedJob);

        Assert.Equal("products.xlsx", _repository.CreatedJob.FileName);
        Assert.Equal(BulkUploadStatus.Pending, _repository.CreatedJob.Status);

        Assert.Equal(2, _repository.CreatedJob.Files.Count);

        Assert.Equal(1, _repository.CreateJobCallCount);
        Assert.Equal(1, _unitOfWork.SaveChangesCallCount);
        Assert.Equal(2, _storage.VerifyCallCount);

        Assert.All(
            _repository.CreatedJob.Files,
            file => Assert.Equal(jobId, file.BulkUploadJobId));
    }

    [Fact]
    public async Task CreateJobFromStagedFilesAsync_ExcelMissing_ShouldThrow()
    {
        var files = new List<BulkUploadStagedFileInput>
        {
            new("product.jpg", "image/jpeg", 2048, "Image", "image-blob")
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().CreateJobFromStagedFilesAsync(files));

        Assert.Equal(0, _repository.CreateJobCallCount);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task CreateJobFromStagedFilesAsync_StagedFileMissing_ShouldThrow()
    {
        _storage.VerifyResult = false;

        var files = new List<BulkUploadStagedFileInput>
        {
            new("products.xlsx", ExcelContentType, 1024, "Excel", "excel-blob")
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().CreateJobFromStagedFilesAsync(files));

        Assert.Equal(1, _storage.VerifyCallCount);
        Assert.Equal(0, _repository.CreateJobCallCount);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    [Fact]
    public async Task CreateJobFromStagedFilesAsync_SizeMismatch_ShouldThrow()
    {
        _storage.VerifyResult = false;

        var files = new List<BulkUploadStagedFileInput>
        {
            new("products.xlsx", ExcelContentType, 1024, "Excel", "excel-blob")
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().CreateJobFromStagedFilesAsync(files));

        Assert.Equal(0, _repository.CreateJobCallCount);
        Assert.Equal(0, _unitOfWork.SaveChangesCallCount);
    }

    private sealed class FakeBulkUploadRepository : IBulkUploadRepository
    {
        public BulkUploadJob? CreatedJob { get; private set; }
        public int CreateJobCallCount { get; private set; }

        public Task<BulkUploadJob> CreateJobAsync(
    BulkUploadJob job,
    CancellationToken cancellationToken = default)
{
    CreateJobCallCount++;

    job.Id = 1;

    foreach (var file in job.Files)
    {
        file.BulkUploadJobId = job.Id;
        file.BulkUploadJob = job;
    }

    CreatedJob = job;

    return Task.FromResult(job);
}

        public Task<BulkUploadJob?> GetByIdAsync(
            int jobId,
            CancellationToken cancellationToken = default)
            => Task.FromResult<BulkUploadJob?>(null);

        public Task<BulkUploadJob?> ClaimNextPendingJobAsync(
            CancellationToken cancellationToken = default)
            => Task.FromResult<BulkUploadJob?>(null);

        public Task<bool> TryRenewLeaseAsync(
            int jobId,
            string claimToken,
            TimeSpan leaseDuration,
            CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<bool> IsClaimOwnerAsync(
            int jobId,
            string claimToken,
            CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task<bool> UpdateJobIfOwnedAsync(
            BulkUploadJob job,
            string claimToken,
            CancellationToken cancellationToken = default)
            => Task.FromResult(false);

        public Task UpdateJobAsync(
            BulkUploadJob job,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task AddErrorAsync(
            BulkUploadError error,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task AddFileAsync(
            BulkUploadFile file,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }

    private sealed class FakeUnitOfWork : IUnitOfWork
    {
        public int SaveChangesCallCount { get; private set; }

        public Task<int> SaveChangesAsync()
        {
            SaveChangesCallCount++;
            return Task.FromResult(1);
        }

        public Task<ITransaction> BeginTransactionAsync()
            => throw new NotImplementedException();
    }

    private sealed class FakeBulkUploadStorageService
        : IBulkUploadStorageService
    {
        public int VerifyCallCount { get; private set; }
        public bool VerifyResult { get; set; } = true;

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
        {
            VerifyCallCount++;
            return Task.FromResult(VerifyResult);
        }

        public Task<string> UploadAsync(
            Stream fileStream,
            string fileName,
            string? contentType)
            => throw new NotImplementedException();

        public Task<(Stream Stream, string ContentType)> DownloadAsync(
            string fileUrl)
            => throw new NotImplementedException();

        public Task DeleteAsync(string fileUrl)
            => Task.CompletedTask;
    }
}
