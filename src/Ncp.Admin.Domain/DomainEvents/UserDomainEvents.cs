using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.DomainEvents;

/// <summary>
/// 用户已创建（领域事件）
/// </summary>
/// <param name="User">新用户聚合根</param>
public record UserCreatedDomainEvent(User User) : IDomainEvent;

/// <summary>
/// 用户已请求追加为部门负责人（用于新增用户表单的快捷入口，实际关系仍由部门聚合维护）。
/// </summary>
/// <param name="UserId">用户 ID。</param>
/// <param name="DeptId">目标部门 ID。</param>
/// <param name="SetAsDefault">是否同步设为默认负责人。</param>
public record UserDeptResponsibleUserAssignmentRequestedDomainEvent(
    UserId UserId,
    DeptId DeptId,
    bool SetAsDefault) : IDomainEvent;

/// <summary>
/// 用户已请求清理部门负责人关联（用于编辑用户取消负责人身份）。
/// </summary>
/// <param name="UserId">用户 ID。</param>
public record UserDeptResponsibleUserClearRequestedDomainEvent(UserId UserId) : IDomainEvent;

/// <summary>
/// 用户已标记为离职（首次由在职变为离职时发布；用于清除部门负责人关联、客户协作订单移交等）
/// </summary>
/// <param name="UserId">用户 ID</param>
public record UserResignedDomainEvent(UserId UserId) : IDomainEvent;

/// <summary>
/// 用户已软删除（用于清除部门负责人关联、客户协作订单移交等）
/// </summary>
/// <param name="UserId">用户 ID</param>
public record UserSoftDeletedDomainEvent(UserId UserId) : IDomainEvent;

/// <summary>
/// 用户首次登录（用于触发新员工入职公告通知）。
/// </summary>
/// <param name="UserId">首次登录用户 ID。</param>
/// <param name="LoginTime">本次登录时间。</param>
public record UserFirstLoggedInDomainEvent(UserId UserId, DateTimeOffset LoginTime) : IDomainEvent;
