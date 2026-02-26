using Ncp.Admin.Domain.AggregatesModel.VehicleAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 车辆仓储接口
/// </summary>
public interface IVehicleRepository : IRepository<Vehicle, VehicleId> { }

/// <summary>
/// 车辆仓储实现
/// </summary>
public class VehicleRepository(ApplicationDbContext context)
    : RepositoryBase<Vehicle, VehicleId, ApplicationDbContext>(context), IVehicleRepository { }
