using Ncp.Admin.Domain.AggregatesModel.VehicleAggregate;

namespace Ncp.Admin.Domain.DomainEvents;

/// <summary>
/// 用车预订已创建
/// </summary>
public record VehicleBookingCreatedDomainEvent(VehicleBooking Booking) : IDomainEvent;

/// <summary>
/// 用车预订已取消
/// </summary>
public record VehicleBookingCancelledDomainEvent(VehicleBooking Booking) : IDomainEvent;

/// <summary>
/// 用车预订已完成
/// </summary>
public record VehicleBookingCompletedDomainEvent(VehicleBooking Booking) : IDomainEvent;
