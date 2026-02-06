using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 流程定义仓储接口
/// </summary>
public interface IWorkflowDefinitionRepository : IRepository<WorkflowDefinition, WorkflowDefinitionId> { }

/// <summary>
/// 流程定义仓储实现
/// </summary>
public class WorkflowDefinitionRepository(ApplicationDbContext context)
    : RepositoryBase<WorkflowDefinition, WorkflowDefinitionId, ApplicationDbContext>(context), IWorkflowDefinitionRepository { }
