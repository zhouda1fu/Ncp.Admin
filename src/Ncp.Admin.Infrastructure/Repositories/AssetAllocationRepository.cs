using Ncp.Admin.Domain.AggregatesModel.AssetAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 资产领用记录仓储接口
/// </summary>
public interface IAssetAllocationRepository : IRepository<AssetAllocation, AssetAllocationId> { }

/// <summary>
/// 资产领用记录仓储实现
/// </summary>
public class AssetAllocationRepository(ApplicationDbContext context)
    : RepositoryBase<AssetAllocation, AssetAllocationId, ApplicationDbContext>(context), IAssetAllocationRepository { }
