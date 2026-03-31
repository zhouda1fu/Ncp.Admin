using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.DomainEvents.UserEvents;

/// <summary>
/// 用户部门主管标识变更领域事件（用于同步更新 Dept 聚合的 ManagerId）
/// </summary>
/// <param name="UserId">用户ID</param>
/// <param name="DeptId">部门ID</param>
/// <param name="IsDeptManager">是否为该部门主管</param>
public record UserDeptManagerChangedDomainEvent(UserId UserId, DeptId DeptId, bool IsDeptManager) : IDomainEvent;
