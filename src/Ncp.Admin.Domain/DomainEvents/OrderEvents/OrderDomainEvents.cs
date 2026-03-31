using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;

namespace Ncp.Admin.Domain.DomainEvents.OrderEvents;

/// <summary>
/// 订单已创建
/// </summary>
public record OrderCreatedDomainEvent(Order Order) : IDomainEvent;

/// <summary>
/// 订单主数据、明细与按分类合同优惠已与聚合内状态一致（在 <see cref="Order.SyncOrderCategories"/> 完成后发布；含新建订单首次同步分类）
/// </summary>
public record OrderUpdatedDomainEvent(Order Order) : IDomainEvent;

/// <summary>
/// 订单已删除（软删）
/// </summary>
public record OrderDeletedDomainEvent(Order Order) : IDomainEvent;

/// <summary>
/// 订单已请求提交审批
/// </summary>
/// <param name="Order">订单聚合</param>
/// <param name="Remark">提交流程备注</param>
public record OrderSubmitRequestedDomainEvent(Order Order, string Remark) : IDomainEvent;
