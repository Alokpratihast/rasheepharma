using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text.Json;
using RashePharma.Application.Interfaces.Services;

namespace RashePharma.Infrastructure.Services;

/// <summary>Private, chunked local storage shared by the API and bulk worker.</summary>
public sealed class LocalFileBulkUploadStorageService : IBulkUploadStorageService
{
    public const int ChunkSizeBytes = 8 * 1024 * 1024;
    private static readonly HashSet<string> AllowedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".xlsx", ".jpg", ".jpeg", ".png", ".webp", ".gif"
    };
    private static readonly ConcurrentDictionary<string, SemaphoreSlim> ChunkLocks = new(StringComparer.Ordinal);
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly LocalFileStorageRoot _storage;

    public LocalFileBulkUploadStorageService(LocalFileStorageRoot storage) => _storage = storage;

    public Task EnsureContainerExistsAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _ = _storage.GetDirectory("bulk-staging");
        _ = _storage.GetDirectory("bulk-metadata");
        _ = _storage.GetDirectory("bulk-chunks");
        return Task.CompletedTask;
    }

    public async Task<BulkUploadStorageTarget> CreateUploadTargetAsync(
        string fileName,
        long expectedFileSize,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var extension = Path.GetExtension(Path.GetFileName(fileName)).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension) || expectedFileSize <= 0)
            throw new ArgumentException("Staged file metadata is invalid.", nameof(fileName));

        await EnsureContainerExistsAsync(cancellationToken);
        var blobName = $"{Guid.NewGuid():N}{extension}";
        var id = GetId(blobName);
        var manifest = new UploadManifest
        {
            FileSize = expectedFileSize,
            CreatedAtUtc = DateTime.UtcNow
        };
        var manifestPath = GetManifestPath(id);
        await WriteJsonAtomicallyAsync(manifestPath, manifest, cancellationToken);

        var uploadUrl = $"/admin/bulk-upload/files/{Uri.EscapeDataString(blobName)}";
        return new BulkUploadStorageTarget(
            blobName,
            _storage.CreateKey("bulk-staging", blobName),
            uploadUrl);
    }

    public async Task WriteUploadChunkAsync(
        string blobName,
        int chunkIndex,
        long expectedFileSize,
        Stream chunk,
        CancellationToken cancellationToken = default)
    {
        var id = GetId(blobName);
        var manifestPath = GetManifestPath(id);
        if (!File.Exists(manifestPath))
            throw new FileNotFoundException("Bulk upload session has expired or does not exist.");

        var manifest = await ReadManifestAsync(manifestPath, cancellationToken);
        if (manifest.FileSize != expectedFileSize)
            throw new InvalidOperationException("Upload size does not match the created upload target.");

        var chunkCount = checked((int)((manifest.FileSize + ChunkSizeBytes - 1) / ChunkSizeBytes));
        if (chunkIndex < 0 || chunkIndex >= chunkCount)
            throw new ArgumentOutOfRangeException(nameof(chunkIndex), "Chunk index is outside the file bounds.");

        var expectedLength = chunkIndex == chunkCount - 1
            ? manifest.FileSize - (long)chunkIndex * ChunkSizeBytes
            : ChunkSizeBytes;
        var lockKey = $"{id}:{chunkIndex}";
        var gate = ChunkLocks.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellationToken);
        try
        {
            var chunkDirectory = GetChunkDirectory(id);
            Directory.CreateDirectory(chunkDirectory);
            var destination = Path.Combine(chunkDirectory, $"{chunkIndex:D8}.part");
            var temporary = Path.Combine(chunkDirectory, $".{Guid.NewGuid():N}.tmp");
            try
            {
                await using (var output = new FileStream(
                    temporary,
                    FileMode.CreateNew,
                    FileAccess.Write,
                    FileShare.None,
                    64 * 1024,
                    FileOptions.Asynchronous | FileOptions.SequentialScan))
                {
                    await chunk.CopyToAsync(output, 64 * 1024, cancellationToken);
                    await output.FlushAsync(cancellationToken);
                    if (output.Length != expectedLength)
                        throw new InvalidOperationException($"Chunk {chunkIndex} has an invalid byte length.");
                }

                File.Move(temporary, destination, overwrite: true);
            }
            finally
            {
                LocalFileStorageRoot.TryDelete(temporary);
            }
        }
        finally
        {
            gate.Release();
        }
    }

    public string GetStagedFileUrl(string blobName) => _storage.CreateKey("bulk-staging", blobName);

    public async Task<bool> VerifyStagedFileAsync(
        string blobName,
        long expectedFileSize,
        CancellationToken cancellationToken = default)
    {
        var destination = _storage.ResolveKey(GetStagedFileUrl(blobName), "bulk-staging");
        if (File.Exists(destination))
            return new FileInfo(destination).Length == expectedFileSize && await HasAllowedSignatureAsync(destination, cancellationToken);

        var id = GetId(blobName);
        var manifestPath = GetManifestPath(id);
        if (!File.Exists(manifestPath)) return false;
        var manifest = await ReadManifestAsync(manifestPath, cancellationToken);
        if (manifest.FileSize != expectedFileSize) return false;

        var chunkCount = checked((int)((expectedFileSize + ChunkSizeBytes - 1) / ChunkSizeBytes));
        var chunkDirectory = GetChunkDirectory(id);
        if (!Directory.Exists(chunkDirectory)) return false;

        for (var index = 0; index < chunkCount; index++)
        {
            var chunkPath = Path.Combine(chunkDirectory, $"{index:D8}.part");
            var expectedLength = index == chunkCount - 1
                ? expectedFileSize - (long)index * ChunkSizeBytes
                : ChunkSizeBytes;
            if (!File.Exists(chunkPath) || new FileInfo(chunkPath).Length != expectedLength)
                return false;
        }

        var temporary = Path.Combine(Path.GetDirectoryName(destination)!, $".{Guid.NewGuid():N}.tmp");
        try
        {
            await using (var output = new FileStream(
                temporary,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                128 * 1024,
                FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                for (var index = 0; index < chunkCount; index++)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await using var input = new FileStream(
                        Path.Combine(chunkDirectory, $"{index:D8}.part"),
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.Read,
                        64 * 1024,
                        FileOptions.Asynchronous | FileOptions.SequentialScan);
                    await input.CopyToAsync(output, 64 * 1024, cancellationToken);
                }
                await output.FlushAsync(cancellationToken);
            }

            if (new FileInfo(temporary).Length != expectedFileSize ||
                !await HasAllowedSignatureAsync(temporary, cancellationToken))
                return false;

            File.Move(temporary, destination, overwrite: true);
            Directory.Delete(chunkDirectory, recursive: true);
            LocalFileStorageRoot.TryDelete(manifestPath);
            return true;
        }
        finally
        {
            LocalFileStorageRoot.TryDelete(temporary);
        }
    }

    public async Task<string> UploadAsync(Stream fileStream, string fileName, string? contentType)
    {
        var extension = Path.GetExtension(Path.GetFileName(fileName)).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            throw new ArgumentException("Staged file type is not supported.", nameof(fileName));
        var storedName = $"{Guid.NewGuid():N}{extension}";
        return await _storage.WriteNewFileAsync("bulk-staging", storedName, fileStream);
    }

    public async Task<(Stream Stream, string ContentType)> DownloadAsync(string fileUrl)
    {
        var path = _storage.ResolveKey(fileUrl, "bulk-staging");
        if (!File.Exists(path)) throw new FileNotFoundException("Staged upload file was not found.");
        var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
            64 * 1024, FileOptions.Asynchronous | FileOptions.SequentialScan);
        return (stream, LocalFileStorageRoot.ContentTypeFor(path));
    }

    public Task DeleteAsync(string fileUrl)
    {
        var path = _storage.ResolveKey(fileUrl, "bulk-staging");
        LocalFileStorageRoot.TryDelete(path);
        var id = GetId(Path.GetFileName(path));
        var chunks = GetChunkDirectory(id);
        if (Directory.Exists(chunks)) Directory.Delete(chunks, recursive: true);
        LocalFileStorageRoot.TryDelete(GetManifestPath(id));
        return Task.CompletedTask;
    }

    public Task CleanupExpiredUploadsAsync(TimeSpan maxAge, CancellationToken cancellationToken = default)
    {
        var directory = _storage.GetDirectory("bulk-metadata");
        var cutoff = DateTime.UtcNow - maxAge;
        foreach (var manifestPath in Directory.EnumerateFiles(directory, "*.json"))
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (File.GetLastWriteTimeUtc(manifestPath) > cutoff) continue;
            var id = Path.GetFileNameWithoutExtension(manifestPath);
            var chunks = GetChunkDirectory(id);
            if (Directory.Exists(chunks)) Directory.Delete(chunks, recursive: true);
            LocalFileStorageRoot.TryDelete(manifestPath);
        }
        return Task.CompletedTask;
    }

    private string GetManifestPath(string id) => _storage.GetPath("bulk-metadata", $"{id}.json");

    private string GetChunkDirectory(string id)
    {
        if (!Guid.TryParseExact(id, "N", out _))
            throw new ArgumentException("Upload identifier is invalid.", nameof(id));
        var directory = Path.GetFullPath(Path.Combine(_storage.GetDirectory("bulk-chunks"), id));
        if (!directory.StartsWith(_storage.GetDirectory("bulk-chunks") + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Upload path is invalid.", nameof(id));
        return directory;
    }

    private static string GetId(string blobName)
    {
        if (string.IsNullOrWhiteSpace(blobName) ||
            !string.Equals(Path.GetFileName(blobName), blobName, StringComparison.Ordinal) ||
            !Guid.TryParseExact(Path.GetFileNameWithoutExtension(blobName), "N", out var id) ||
            !AllowedExtensions.Contains(Path.GetExtension(blobName)))
            throw new ArgumentException("Staging file identifier is invalid.", nameof(blobName));
        return id.ToString("N");
    }

    private static async Task<bool> HasAllowedSignatureAsync(string path, CancellationToken cancellationToken)
    {
        var extension = Path.GetExtension(path).ToLowerInvariant();
        var header = new byte[12];
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
            4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
        var read = await stream.ReadAsync(header.AsMemory(), cancellationToken);
        return extension switch
        {
            ".xlsx" => read >= 4 && header[0] == 0x50 && header[1] == 0x4B && header[2] == 0x03 && header[3] == 0x04,
            ".jpg" or ".jpeg" => read >= 3 && header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF,
            ".png" => read >= 8 && header.AsSpan(0, 8).SequenceEqual(new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }),
            ".gif" => read >= 6 && (header.AsSpan(0, 6).SequenceEqual("GIF87a"u8) || header.AsSpan(0, 6).SequenceEqual("GIF89a"u8)),
            ".webp" => read >= 12 && header.AsSpan(0, 4).SequenceEqual("RIFF"u8) && header.AsSpan(8, 4).SequenceEqual("WEBP"u8),
            _ => false
        };
    }

    private static async Task<UploadManifest> ReadManifestAsync(string path, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read,
            4096, FileOptions.Asynchronous | FileOptions.SequentialScan);
        return await JsonSerializer.DeserializeAsync<UploadManifest>(stream, JsonOptions, cancellationToken)
            ?? throw new InvalidDataException("Upload manifest is invalid.");
    }

    private static async Task WriteJsonAtomicallyAsync(string path, UploadManifest manifest, CancellationToken cancellationToken)
    {
        var temporary = path + $".{Guid.NewGuid():N}.tmp";
        try
        {
            await using (var stream = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write, FileShare.None,
                4096, FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                await JsonSerializer.SerializeAsync(stream, manifest, JsonOptions, cancellationToken);
                await stream.FlushAsync(cancellationToken);
            }
            File.Move(temporary, path);
        }
        finally
        {
            LocalFileStorageRoot.TryDelete(temporary);
        }
    }

    private sealed class UploadManifest
    {
        public long FileSize { get; set; }
        public DateTime CreatedAtUtc { get; set; }
    }
}
