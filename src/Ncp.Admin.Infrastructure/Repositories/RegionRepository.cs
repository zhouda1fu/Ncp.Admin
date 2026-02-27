using Ncp.Admin.Domain.AggregatesModel.RegionAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 区域仓储接口
/// </summary>
public interface IRegionRepository : IRepository<Region, RegionId> { }

/// <summary>
/// 区域仓储实现
/// </summary>
public class RegionRepository(ApplicationDbContext context)
    : RepositoryBase<Region, RegionId, ApplicationDbContext>(context), IRegionRepository { }
