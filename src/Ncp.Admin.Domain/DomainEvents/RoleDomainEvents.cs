using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;

namespace Ncp.Admin.Domain.DomainEvents;

/// <summary>
/// 角色信息变更领域事件
/// </summary>
/// <param name="Role">角色聚合根</param>
public record RoleInfoChangedDomainEvent(Role Role) : IDomainEvent;

/// <summary>
/// 角色权限变更领域事件
/// </summary>
/// <param name="Role">角色聚合根</param>
public record RolePermissionChangedDomainEvent(Role Role) : IDomainEvent;

