namespace Ncp.Admin.Domain.AggregatesModel.DocumentAggregate;

/// <summary>
/// 文档版本ID（强类型ID）
/// </summary>
public partial record DocumentVersionId : IGuidStronglyTypedId;

/// <summary>
/// 文档版本（聚合内实体），存储每个版本的文件引用
/// </summary>
public class DocumentVersion : Entity<DocumentVersionId>
{
    protected DocumentVersion() { }

    /// <summary>
    /// 版本号，从 1 递增
    /// </summary>
    public int VersionNumber { get; private set; }

    /// <summary>
    /// 文件存储键（IFileStorageService 返回的 key）
    /// </summary>
    public string FileStorageKey { get; private set; } = string.Empty;

    /// <summary>
    /// 原始文件名
    /// </summary>
    public string FileName { get; private set; } = string.Empty;

    /// <summary>
    /// 文件大小（字节）
    /// </summary>
    public long FileSize { get; private set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    internal DocumentVersion(int versionNumber, string fileStorageKey, string fileName, long fileSize)
    {
        VersionNumber = versionNumber;
        FileStorageKey = fileStorageKey ?? string.Empty;
        FileName = fileName ?? string.Empty;
        FileSize = fileSize;
        CreatedAt = DateTimeOffset.UtcNow;
    }
}
