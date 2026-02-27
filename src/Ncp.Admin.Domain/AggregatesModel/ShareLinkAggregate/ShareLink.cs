using Ncp.Admin.Domain.AggregatesModel.DocumentAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.ShareLinkAggregate;

/// <summary>
/// 共享链接ID（强类型ID）
/// </summary>
public partial record ShareLinkId : IGuidStronglyTypedId;

/// <summary>
/// 文档共享链接聚合根
/// </summary>
public class ShareLink : Entity<ShareLinkId>, IAggregateRoot
{
    protected ShareLink() { }

    /// <summary>
    /// 关联文档ID
    /// </summary>
    public DocumentId DocumentId { get; private set; } = default!;

    /// <summary>
    /// 分享令牌（唯一，用于访问链接）
    /// </summary>
    public string Token { get; private set; } = string.Empty;

    /// <summary>
    /// 创建人用户ID
    /// </summary>
    public UserId CreatorId { get; private set; } = default!;

    /// <summary>
    /// 过期时间（null 表示永久有效）
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; private set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }

    /// <summary>
    /// 创建共享链接
    /// </summary>
    public ShareLink(DocumentId documentId, string token, UserId creatorId, DateTimeOffset? expiresAt = null)
    {
        DocumentId = documentId;
        Token = token ;
        CreatorId = creatorId;
        ExpiresAt = expiresAt;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 是否已过期
    /// </summary>
    public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value < DateTimeOffset.UtcNow;
}
