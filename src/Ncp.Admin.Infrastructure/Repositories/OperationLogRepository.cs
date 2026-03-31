using Ncp.Admin.Domain.AggregatesModel.OperationLogAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 操作日志仓储接口
/// </summary>
public interface IOperationLogRepository : IRepository<OperationLog, OperationLogId> { }

/// <summary>
/// 操作日志仓储实现
/// </summary>
public class OperationLogRepository(ApplicationDbContext context)
    : RepositoryBase<OperationLog, OperationLogId, ApplicationDbContext>(context), IOperationLogRepository { }
