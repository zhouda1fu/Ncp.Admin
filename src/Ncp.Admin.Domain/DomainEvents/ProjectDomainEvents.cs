using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;

namespace Ncp.Admin.Domain.DomainEvents;

/// <summary>
/// 项目已创建
/// </summary>
public record ProjectCreatedDomainEvent(Project Project) : IDomainEvent;

/// <summary>
/// 项目主档或联系人、跟进记录等聚合内数据已变更
/// </summary>
public record ProjectUpdatedDomainEvent(Project Project) : IDomainEvent;

/// <summary>
/// 项目已归档
/// </summary>
public record ProjectArchivedDomainEvent(Project Project) : IDomainEvent;

/// <summary>
/// 项目已重新激活
/// </summary>
public record ProjectActivatedDomainEvent(Project Project) : IDomainEvent;

/// <summary>
/// 项目跟进记录已添加
/// </summary>
public record ProjectFollowUpRecordAddedDomainEvent(Project Project, ProjectFollowUpRecord Record) : IDomainEvent;

/// <summary>
/// 项目跟进记录已更新
/// </summary>
public record ProjectFollowUpRecordUpdatedDomainEvent(Project Project, ProjectFollowUpRecord Record) : IDomainEvent;

/// <summary>
/// 项目跟进记录已移除
/// </summary>
public record ProjectFollowUpRecordRemovedDomainEvent(Project Project, ProjectFollowUpRecord Record) : IDomainEvent;

/// <summary>
/// 项目联系人已添加
/// </summary>
public record ProjectContactAddedDomainEvent(Project Project, ProjectContact Contact) : IDomainEvent;

/// <summary>
/// 项目联系人已更新
/// </summary>
public record ProjectContactUpdatedDomainEvent(Project Project, ProjectContact Contact) : IDomainEvent;

/// <summary>
/// 项目联系人已移除
/// </summary>
public record ProjectContactRemovedDomainEvent(Project Project, ProjectContact Contact) : IDomainEvent;
