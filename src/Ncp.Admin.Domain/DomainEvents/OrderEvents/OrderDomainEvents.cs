using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;

namespace Ncp.Admin.Domain.DomainEvents.OrderEvents;

/// <summary>
/// 订单已创建
/// </summary>
public record OrderCreatedDomainEvent(Order Order) : IDomainEvent;

/// <summary>
/// 订单已更新
/// </summary>
public record OrderUpdatedDomainEvent(Order Order) : IDomainEvent;

/// <summary>
/// 订单已删除（软删）
/// </summary>
public record OrderDeletedDomainEvent(Order Order) : IDomainEvent;
