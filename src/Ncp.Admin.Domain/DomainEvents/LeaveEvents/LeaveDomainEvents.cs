using Ncp.Admin.Domain.AggregatesModel.LeaveRequestAggregate;

namespace Ncp.Admin.Domain.DomainEvents.LeaveEvents;

/// <summary>
/// 请假申请已审批通过领域事件
/// </summary>
public record LeaveRequestApprovedDomainEvent(LeaveRequest LeaveRequest) : IDomainEvent;

/// <summary>
/// 请假申请已创建领域事件
/// </summary>
public record LeaveRequestCreatedDomainEvent(LeaveRequest LeaveRequest) : IDomainEvent;

/// <summary>
/// 请假申请已提交审批领域事件
/// </summary>
public record LeaveRequestSubmittedDomainEvent(LeaveRequest LeaveRequest) : IDomainEvent;
