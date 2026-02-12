namespace Ncp.Admin.Infrastructure.Services;

/// <summary>
/// 文件存储服务抽象，支持本地、MinIO、OSS 等实现。
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// 上传文件，返回可访问路径或存储键。
    /// </summary>
    /// <param name="stream">文件流</param>
    /// <param name="fileName">原始文件名（用于扩展名等）</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>存储键或相对路径，用于后续下载/删除</returns>
    Task<string> UploadAsync(Stream stream, string fileName, CancellationToken cancellationToken = default);

    /// <summary>
    /// 下载文件。
    /// </summary>
    /// <param name="fileKey">上传时返回的 key 或路径</param>
    /// <param name="cancellationToken">取消令牌</param>
    /// <returns>文件流，若不存在则返回 null</returns>
    Task<Stream?> DownloadAsync(string fileKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// 删除文件。
    /// </summary>
    /// <param name="fileKey">上传时返回的 key 或路径</param>
    /// <param name="cancellationToken">取消令牌</param>
    Task DeleteAsync(string fileKey, CancellationToken cancellationToken = default);
}
