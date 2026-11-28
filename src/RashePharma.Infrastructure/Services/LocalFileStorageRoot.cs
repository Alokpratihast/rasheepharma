using System.Text.RegularExpressions;

namespace RashePharma.Infrastructure.Services;

/// <summary>Resolves opaque local-storage keys to files under one private root.</summary>
public sealed class LocalFileStorageRoot
{
    private static readonly Regex SafeFileName = new(
        @"\A[a-zA-Z0-9_-]+(?:\.[a-zA-Z0-9]+)?\z",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public LocalFileStorageRoot(string rootPath)
    {
        if (string.IsNullOrWhiteSpace(rootPath))
            throw new ArgumentException("Storage root path is required.", nameof(rootPath));

        RootPath = Path.GetFullPath(rootPath);
        Directory.CreateDirectory(RootPath);
    }

    public string RootPath { get; }

    public string GetDirectory(string prefix)
    {
        ValidatePrefix(prefix);
        var path = Path.GetFullPath(Path.Combine(RootPath, prefix));
        EnsureContained(path);
        Directory.CreateDirectory(path);
        return path;
    }

    public string GetPath(string prefix, string fileName)
    {
        ValidatePrefix(prefix);
        if (string.IsNullOrWhiteSpace(fileName) ||
            !string.Equals(Path.GetFileName(fileName), fileName, StringComparison.Ordinal) ||
            !SafeFileName.IsMatch(fileName))
        {
            throw new ArgumentException("Storage file name is invalid.", nameof(fileName));
        }

        var directory = Path.GetFullPath(Path.Combine(RootPath, prefix));
        var path = Path.GetFullPath(Path.Combine(directory, fileName));
        EnsureContained(path);
        Directory.CreateDirectory(directory);
        return path;
    }

    public string CreateKey(string prefix, string fileName)
    {
        _ = GetPath(prefix, fileName);
        return $"local://{prefix}/{Uri.EscapeDataString(fileName)}";
    }

    public string ResolveKey(string key, string expectedPrefix)
    {
        if (!Uri.TryCreate(key, UriKind.Absolute, out var uri) ||
            !string.Equals(uri.Scheme, "local", StringComparison.OrdinalIgnoreCase) ||
            !string.Equals(uri.Host, expectedPrefix, StringComparison.OrdinalIgnoreCase) ||
            uri.Query.Length > 0 || uri.Fragment.Length > 0)
        {
            throw new ArgumentException("Storage key is outside the configured storage area.", nameof(key));
        }

        var fileName = Uri.UnescapeDataString(uri.AbsolutePath.TrimStart('/'));
        return GetPath(expectedPrefix, fileName);
    }

    public async Task<string> WriteNewFileAsync(
        string prefix,
        string fileName,
        Stream source,
        CancellationToken cancellationToken = default)
    {
        var destination = GetPath(prefix, fileName);
        var temporary = Path.Combine(
            Path.GetDirectoryName(destination)!,
            $".{Guid.NewGuid():N}.tmp");

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
                await source.CopyToAsync(output, 64 * 1024, cancellationToken);
                await output.FlushAsync(cancellationToken);
            }

            File.Move(temporary, destination);
            return CreateKey(prefix, fileName);
        }
        catch
        {
            TryDelete(temporary);
            throw;
        }
    }

    public static string ContentTypeFor(string fileName) =>
        Path.GetExtension(fileName).ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".webp" => "image/webp",
            ".gif" => "image/gif",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".pdf" => "application/pdf",
            _ => "application/octet-stream"
        };

    public static void TryDelete(string path)
    {
        try
        {
            if (File.Exists(path)) File.Delete(path);
        }
        catch (IOException) { }
        catch (UnauthorizedAccessException) { }
    }

    private void EnsureContained(string path)
    {
        var rootPrefix = RootPath.EndsWith(Path.DirectorySeparatorChar)
            ? RootPath
            : RootPath + Path.DirectorySeparatorChar;
        if (!path.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException("Resolved storage path escapes the configured root.");
    }

    private static void ValidatePrefix(string prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix) ||
            !string.Equals(Path.GetFileName(prefix), prefix, StringComparison.Ordinal) ||
            !SafeFileName.IsMatch(prefix) || prefix.Contains('.'))
        {
            throw new ArgumentException("Storage prefix is invalid.", nameof(prefix));
        }
    }
}
