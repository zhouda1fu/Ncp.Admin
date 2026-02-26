using Ncp.Admin.Domain.AggregatesModel.AssetAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 资产仓储接口
/// </summary>
public interface IAssetRepository : IRepository<Asset, AssetId> { }

/// <summary>
/// 资产仓储实现
/// </summary>
public class AssetRepository(ApplicationDbContext context)
    : RepositoryBase<Asset, AssetId, ApplicationDbContext>(context), IAssetRepository { }
