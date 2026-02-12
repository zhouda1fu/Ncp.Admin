using Ncp.Admin.Domain.AggregatesModel.PositionAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 岗位仓储接口
/// </summary>
public interface IPositionRepository : IRepository<Position, PositionId> { }

/// <summary>
/// 岗位仓储实现
/// </summary>
public class PositionRepository(ApplicationDbContext context) : RepositoryBase<Position, PositionId, ApplicationDbContext>(context), IPositionRepository { }
