
using Microsoft.Extensions.Logging.Abstractions;
using RashePharma.Application.Interfaces.Repositories;
using RashePharma.Application.Interfaces.Services;
using RashePharma.Application.Interfaces;
using RashePharma.Application.Services;
using Xunit;

namespace RashePharma.Api.Tests;

public class BulkUploadServiceManifestTests
{
    private const string ExcelContentType =
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private readonly FakeBulkUploadStorageService _storage = new();

    private BulkUploadService CreateService()
    {
        return new BulkUploadService(
            null!,
            null!,
            null!,
            null!,
            null!,
            null!,
            _storage,
            null!,
            null!,
            null!,
            NullLogger<BulkUploadService>.Instance);
    }

    [Fact]
    public async Task CreateUploadTargetsAsync_ValidManifest_ShouldCreateTargets()
    {
        var files = new List<BulkUploadFileDescriptor>
        {
            new("products.xlsx", ExcelContentType, 1024, "Excel"),
            new("product.jpg", "image/jpeg", 2048, "Image")
        };

        var result = await CreateService()
            .CreateUploadTargetsAsync(files);

        Assert.Equal(2, result.Count);
        Assert.Equal(1, _storage.EnsureContainerCallCount);
        Assert.Equal(2, _storage.CreateTargetCallCount);
        Assert.Equal("products.xlsx", result[0].FileName);
        Assert.Equal("product.jpg", result[1].FileName);
    }

    [Fact]
    public async Task CreateUploadTargetsAsync_ExcelMissing_ShouldThrow()
    {
        var files = new List<BulkUploadFileDescriptor>
        {
            new("product.jpg", "image/jpeg", 1024, "Image")
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().CreateUploadTargetsAsync(files));

        Assert.Equal(0, _storage.EnsureContainerCallCount);
        Assert.Equal(0, _storage.CreateTargetCallCount);
    }

    [Fact]
    public async Task CreateUploadTargetsAsync_InvalidExcelExtension_ShouldThrow()
    {
        var files = new List<BulkUploadFileDescriptor>
        {
            new("products.xls", ExcelContentType, 1024, "Excel")
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().CreateUploadTargetsAsync(files));

        Assert.Equal(0, _storage.EnsureContainerCallCount);
    }

    [Fact]
    public async Task CreateUploadTargetsAsync_ExcelExceeds50MiB_ShouldThrow()
    {
        var files = new List<BulkUploadFileDescriptor>
        {
            new("products.xlsx", ExcelContentType, 50L * 1024 * 1024 + 1, "Excel")
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().CreateUploadTargetsAsync(files));

        Assert.Equal(0, _storage.EnsureContainerCallCount);
    }

    [Fact]
    public async Task CreateUploadTargetsAsync_DuplicateImageNames_ShouldThrow()
    {
        var files = new List<BulkUploadFileDescriptor>
        {
            new("products.xlsx", ExcelContentType, 1024, "Excel"),
            new("product.jpg", "image/jpeg", 2048, "Image"),
            new("PRODUCT.JPG", "image/jpeg", 2048, "Image")
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().CreateUploadTargetsAsync(files));

        Assert.Equal(0, _storage.EnsureContainerCallCount);
    }

    [Fact]
    public async Task CreateUploadTargetsAsync_ImageContentTypeMismatch_ShouldThrow()
    {
        var files = new List<BulkUploadFileDescriptor>
        {
            new("products.xlsx", ExcelContentType, 1024, "Excel"),
            new("product.png", "image/jpeg", 2048, "Image")
        };

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => CreateService().CreateUploadTargetsAsync(files));

        Assert.Equal(0, _storage.EnsureContainerCallCount);
        Assert.Equal(0, _storage.CreateTargetCallCount);
    }

    private sealed class FakeBulkUploadStorageService
        : IBulkUploadStorageService
    {
        public int EnsureContainerCallCount { get; private set; }
        public int CreateTargetCallCount { get; private set; }

        public Task EnsureContainerExistsAsync(
            CancellationToken cancellationToken = default)
        {
            EnsureContainerCallCount++;
            return Task.CompletedTask;
        }

        public Task<BulkUploadStorageTarget> CreateUploadTargetAsync(
            string fileName,
            CancellationToken cancellationToken = default)
        {
            CreateTargetCallCount++;

            return Task.FromResult(
                new BulkUploadStorageTarget(
                    $"staging/{fileName}",
                    $"https://storage.test/{fileName}",
                    $"https://storage.test/upload/{fileName}"));
        }

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
            => Task.FromResult($"https://storage.test/{fileName}");

        public Task<(Stream Stream, string ContentType)> DownloadAsync(
            string fileUrl)
            => Task.FromResult<(Stream, string)>(
                (new MemoryStream(), "application/octet-stream"));

        public Task DeleteAsync(string fileUrl)
            => Task.CompletedTask;
    }
}
