using Ncp.Admin.Domain.AggregatesModel.TaskAggregate;
using TaskEntity = Ncp.Admin.Domain.AggregatesModel.TaskAggregate.Task;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 任务仓储接口
/// </summary>
public interface ITaskRepository : IRepository<TaskEntity, TaskId> { }

/// <summary>
/// 任务仓储实现
/// </summary>
public class TaskRepository(ApplicationDbContext context)
    : RepositoryBase<TaskEntity, TaskId, ApplicationDbContext>(context), ITaskRepository { }
