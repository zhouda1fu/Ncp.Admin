using Ncp.Admin.Infrastructure.Services;

namespace Ncp.Admin.Web.Application.Services.Files;

/// <summary>
/// 兼容老 OA Files 目录的文件存储包装器。
/// </summary>
public class LegacyFilesFileStorageService(
    IFileStorageService inner,
    IWebHostEnvironment environment) : IFileStorageService
{
    private readonly string _legacyFilesRoot = Path.Combine(
        environment.WebRootPath ?? Path.Combine(environment.ContentRootPath, "wwwroot"),
        "Files");

    public Task<string> UploadAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
        => inner.UploadAsync(stream, fileName, cancellationToken);

    public Task DeleteAsync(string fileKey, CancellationToken cancellationToken = default)
        => inner.DeleteAsync(fileKey, cancellationToken);

    public async Task<Stream?> DownloadAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        var legacyStream = TryOpenLegacyFile(fileKey);
        if (legacyStream != null)
            return legacyStream;

        return await inner.DownloadAsync(fileKey, cancellationToken);
    }

    private Stream? TryOpenLegacyFile(string fileKey)
    {
        var normalized = NormalizeLegacyKey(fileKey);
        if (string.IsNullOrWhiteSpace(normalized))
            return null;

        var fullPath = Path.GetFullPath(Path.Combine(
            _legacyFilesRoot,
            normalized.Replace('/', Path.DirectorySeparatorChar)));

        if (!IsPathUnderRoot(fullPath, _legacyFilesRoot) || !File.Exists(fullPath))
            return null;

        return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    private static string NormalizeLegacyKey(string fileKey)
    {
        var normalized = (fileKey ?? string.Empty).Trim().Replace('\\', '/').TrimStart('/');
        if (normalized.StartsWith("Files/", StringComparison.OrdinalIgnoreCase))
            normalized = normalized["Files/".Length..];

        if (normalized.Length == 0
            || Path.IsPathRooted(normalized)
            || normalized.Split('/').Any(segment => segment is "" or "." or ".."))
        {
            return string.Empty;
        }

        return normalized;
    }

    private static bool IsPathUnderRoot(string fullPath, string rootPath)
    {
        var comparison = OperatingSystem.IsWindows()
            ? StringComparison.OrdinalIgnoreCase
            : StringComparison.Ordinal;
        var root = Path.GetFullPath(rootPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;

        return fullPath.StartsWith(root, comparison);
    }
}
