using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.DocumentAggregate;

/// <summary>
/// 文档ID（强类型ID）
/// </summary>
public partial record DocumentId : IGuidStronglyTypedId;

/// <summary>
/// 文档聚合根，含多版本
/// </summary>
public class Document : Entity<DocumentId>, IAggregateRoot
{
    private readonly List<DocumentVersion> _versions = [];

    protected Document() { }

    /// <summary>
    /// 文档标题
    /// </summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>
    /// 创建人用户ID
    /// </summary>
    public UserId CreatorId { get; private set; } = default!;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public UpdateTime UpdateTime { get; private set; } = new UpdateTime(DateTimeOffset.UtcNow);

    /// <summary>
    /// 版本列表（只读）
    /// </summary>
    public IReadOnlyList<DocumentVersion> Versions => _versions.AsReadOnly();

    /// <summary>
    /// 当前最新版本（若尚无版本则为 null）
    /// </summary>
    public DocumentVersion? CurrentVersion => _versions.OrderByDescending(v => v.VersionNumber).FirstOrDefault();

    /// <summary>
    /// 创建文档（带首版）
    /// </summary>
    public Document(string title, UserId creatorId, string fileStorageKey, string fileName, long fileSize)
    {
        Title = title ;
        CreatorId = creatorId;
        CreatedAt = DateTimeOffset.UtcNow;
        _versions.Add(new DocumentVersion(1, fileStorageKey, fileName, fileSize));
    }

    /// <summary>
    /// 更新文档标题
    /// </summary>
    public void UpdateTitle(string title)
    {
        Title = title ;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 添加新版本
    /// </summary>
    public void AddVersion(string fileStorageKey, string fileName, long fileSize)
    {
        var nextVersion = (_versions.Count == 0 ? 0 : _versions.Max(v => v.VersionNumber)) + 1;
        _versions.Add(new DocumentVersion(nextVersion, fileStorageKey, fileName, fileSize));
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }
}
