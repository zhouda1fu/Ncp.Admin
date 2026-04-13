using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;

namespace Ncp.Admin.Domain.DomainEvents;

/// <summary>
/// 部门信息变更领域事件
/// </summary>
/// <param name="Dept">部门聚合根</param>
public record DeptInfoChangedDomainEvent(Dept Dept) : IDomainEvent;

