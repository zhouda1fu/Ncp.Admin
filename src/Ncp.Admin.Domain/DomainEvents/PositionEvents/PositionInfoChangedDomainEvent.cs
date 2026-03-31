using Ncp.Admin.Domain.AggregatesModel.PositionAggregate;

namespace Ncp.Admin.Domain.DomainEvents.PositionEvents;

/// <summary>
/// 岗位信息变更领域事件
/// </summary>
public record PositionInfoChangedDomainEvent(Position Position) : IDomainEvent;
