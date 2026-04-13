using Ncp.Admin.Domain.AggregatesModel.AnnouncementAggregate;

namespace Ncp.Admin.Domain.DomainEvents;

/// <summary>
/// 公告已创建（草稿）
/// </summary>
public record AnnouncementCreatedDomainEvent(Announcement Announcement) : IDomainEvent;

/// <summary>
/// 公告草稿已更新
/// </summary>
public record AnnouncementDraftUpdatedDomainEvent(Announcement Announcement) : IDomainEvent;

/// <summary>
/// 公告已发布
/// </summary>
public record AnnouncementPublishedDomainEvent(Announcement Announcement) : IDomainEvent;
