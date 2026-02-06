using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 流程实例仓储接口
/// </summary>
public interface IWorkflowInstanceRepository : IRepository<WorkflowInstance, WorkflowInstanceId> { }

/// <summary>
/// 流程实例仓储实现
/// </summary>
public class WorkflowInstanceRepository(ApplicationDbContext context)
    : RepositoryBase<WorkflowInstance, WorkflowInstanceId, ApplicationDbContext>(context), IWorkflowInstanceRepository { }
