using Ncp.Admin.Domain.AggregatesModel.VehicleAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 用车预订仓储接口
/// </summary>
public interface IVehicleBookingRepository : IRepository<VehicleBooking, VehicleBookingId>
{
    /// <summary>
    /// 检查指定车辆在给定时段内是否已有有效预订
    /// </summary>
    Task<bool> HasConflictAsync(VehicleId vehicleId, DateTimeOffset start, DateTimeOffset end, VehicleBookingId? excludeId, CancellationToken cancellationToken = default);
}

/// <summary>
/// 用车预订仓储实现
/// </summary>
public class VehicleBookingRepository(ApplicationDbContext context)
    : RepositoryBase<VehicleBooking, VehicleBookingId, ApplicationDbContext>(context), IVehicleBookingRepository
{
    /// <inheritdoc />
    public async Task<bool> HasConflictAsync(VehicleId vehicleId, DateTimeOffset start, DateTimeOffset end, VehicleBookingId? excludeId, CancellationToken cancellationToken = default)
    {
        var query = DbContext.VehicleBookings
            .Where(b => b.VehicleId == vehicleId && b.Status == VehicleBookingStatus.Booked
                && b.StartAt < end && b.EndAt > start);
        if (excludeId != null)
            query = query.Where(b => b.Id != excludeId);
        return await query.AnyAsync(cancellationToken);
    }
}
