using Microsoft.Extensions.Options;

namespace Ncp.Admin.Infrastructure.Services;

/// <summary>
/// 本地文件存储配置
/// </summary>
public class LocalFileStorageOptions
{
    public const string SectionName = "FileStorage:Local";

    /// <summary>
    /// 存储根目录（绝对或相对应用根路径）
    /// </summary>
    public string RootPath { get; set; } = "uploads";

    /// <summary>
    /// 是否通过 Web 静态文件提供访问（即文件在 wwwroot 下时可直接通过 URL 访问）
    /// </summary>
    public bool ServeViaStaticFiles { get; set; }
}

/// <summary>
/// 本地磁盘文件存储实现
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    private readonly string _rootPath;
    private readonly string _basePath;

    public LocalFileStorageService(IOptions<LocalFileStorageOptions> options)
    {
        var root = options.Value.RootPath ?? "uploads";
        _rootPath = Path.IsPathRooted(root) ? root : Path.Combine(AppContext.BaseDirectory, root);
        _basePath = _rootPath;
        if (!Directory.Exists(_basePath))
            Directory.CreateDirectory(_basePath);
    }

    public async Task<string> UploadAsync(Stream stream, string fileName, CancellationToken cancellationToken = default)
    {
        var safeName = Path.GetFileName(fileName);
        if (string.IsNullOrEmpty(safeName))
            safeName = "file";
        var ext = Path.GetExtension(safeName);
        var subDir = DateTime.UtcNow.ToString("yyyyMMdd");
        var dir = Path.Combine(_basePath, subDir);
        if (!Directory.Exists(dir))
            Directory.CreateDirectory(dir);
        var uniqueName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(dir, uniqueName);
        await using var fs = new FileStream(fullPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, true);
        await stream.CopyToAsync(fs, cancellationToken);
        return Path.Combine(subDir, uniqueName).Replace('\\', '/');
    }

    public Task<Stream?> DownloadAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, fileKey.Replace('/', Path.DirectorySeparatorChar));
        if (!File.Exists(fullPath))
            return Task.FromResult<Stream?>(null);
        return Task.FromResult<Stream?>(new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read));
    }

    public Task DeleteAsync(string fileKey, CancellationToken cancellationToken = default)
    {
        var fullPath = Path.Combine(_basePath, fileKey.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(fullPath))
            File.Delete(fullPath);
        return Task.CompletedTask;
    }
}
