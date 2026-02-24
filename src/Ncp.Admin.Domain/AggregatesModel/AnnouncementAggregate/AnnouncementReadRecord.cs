using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.AnnouncementAggregate;

/// <summary>
/// 公告已读记录ID
/// </summary>
public partial record AnnouncementReadRecordId : IGuidStronglyTypedId;

/// <summary>
/// 公告已读记录聚合根，用于追踪用户对某条公告的已读状态
/// </summary>
public class AnnouncementReadRecord : Entity<AnnouncementReadRecordId>, IAggregateRoot
{
    protected AnnouncementReadRecord() { }

    /// <summary>
    /// 关联的公告ID
    /// </summary>
    public AnnouncementId AnnouncementId { get; private set; } = default!;
    /// <summary>
    /// 已读用户ID
    /// </summary>
    public UserId UserId { get; private set; } = default!;
    /// <summary>
    /// 阅读时间
    /// </summary>
    public DateTimeOffset ReadAt { get; init; }

    /// <summary>
    /// 创建一条已读记录
    /// </summary>
    /// <param name="announcementId">公告ID</param>
    /// <param name="userId">用户ID</param>
    public AnnouncementReadRecord(AnnouncementId announcementId, UserId userId)
    {
        AnnouncementId = announcementId;
        UserId = userId;
        ReadAt = DateTimeOffset.UtcNow;
    }
}
