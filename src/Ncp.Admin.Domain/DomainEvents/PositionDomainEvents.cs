using Ncp.Admin.Domain.AggregatesModel.PositionAggregate;

namespace Ncp.Admin.Domain.DomainEvents;

/// <summary>
/// 岗位信息变更领域事件
/// </summary>
/// <param name="Position">岗位聚合根</param>
public record PositionInfoChangedDomainEvent(Position Position) : IDomainEvent;

