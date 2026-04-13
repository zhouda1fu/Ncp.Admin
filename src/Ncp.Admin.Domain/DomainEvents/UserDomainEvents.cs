using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.DomainEvents;

/// <summary>
/// 用户已创建（领域事件）
/// </summary>
/// <param name="User">新用户聚合根</param>
public record UserCreatedDomainEvent(User User) : IDomainEvent;

/// <summary>
/// 用户部门主管标识变更（用于同步更新 Dept 聚合的 ManagerId）
/// </summary>
/// <param name="UserId">用户 ID</param>
/// <param name="DeptId">部门 ID</param>
/// <param name="IsDeptManager">是否为该部门主管</param>
public record UserDeptManagerChangedDomainEvent(UserId UserId, DeptId DeptId, bool IsDeptManager) : IDomainEvent;

/// <summary>
/// 用户离职或删除（用于清除其作为部门主管的关联等）
/// </summary>
/// <param name="UserId">用户 ID</param>
public record UserResignedOrDeletedDomainEvent(UserId UserId) : IDomainEvent;
