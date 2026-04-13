using Ncp.Admin.Domain.AggregatesModel.VehicleAggregate;

namespace Ncp.Admin.Domain.DomainEvents;

/// <summary>
/// 车辆已创建
/// </summary>
public record VehicleCreatedDomainEvent(Vehicle Vehicle) : IDomainEvent;

/// <summary>
/// 车辆主档信息已更新
/// </summary>
public record VehicleUpdatedDomainEvent(Vehicle Vehicle) : IDomainEvent;

/// <summary>
/// 车辆状态已变更（可用/禁用）
/// </summary>
public record VehicleStatusChangedDomainEvent(Vehicle Vehicle) : IDomainEvent;
