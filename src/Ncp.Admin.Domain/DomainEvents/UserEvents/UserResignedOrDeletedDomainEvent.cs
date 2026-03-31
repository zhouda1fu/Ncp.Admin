using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.DomainEvents.UserEvents;

/// <summary>
/// 用户离职或删除领域事件（用于清除其作为部门主管的关联）
/// </summary>
public record UserResignedOrDeletedDomainEvent(UserId UserId) : IDomainEvent;
