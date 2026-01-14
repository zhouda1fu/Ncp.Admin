using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;

namespace Ncp.Admin.Domain.DomainEvents.DeptEvents;

/// <summary>
/// 部门信息变更领域事件
/// </summary>
public record DeptInfoChangedDomainEvent(Dept Dept) : IDomainEvent;
