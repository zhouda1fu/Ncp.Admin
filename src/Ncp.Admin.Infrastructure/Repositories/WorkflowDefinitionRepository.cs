using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;

namespace Ncp.Admin.Infrastructure.Repositories;

/// <summary>
/// 流程定义仓储接口
/// </summary>
public interface IWorkflowDefinitionRepository : IRepository<WorkflowDefinition, WorkflowDefinitionId>
{
    /// <summary>
    /// 获取指定流程定义版本。
    /// </summary>
    Task<WorkflowDefinitionVersion?> GetVersionAsync(
        WorkflowDefinitionVersionId id,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 按流程名称与分类精确匹配（导入 upsert 用）。
    /// </summary>
    Task<WorkflowDefinition?> GetByNameAndCategoryAsync(
        string name,
        string category,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// 流程定义仓储实现
/// </summary>
public class WorkflowDefinitionRepository(ApplicationDbContext context)
    : RepositoryBase<WorkflowDefinition, WorkflowDefinitionId, ApplicationDbContext>(context), IWorkflowDefinitionRepository
{
    /// <inheritdoc />
    public Task<WorkflowDefinitionVersion?> GetVersionAsync(
        WorkflowDefinitionVersionId id,
        CancellationToken cancellationToken = default)
    {
        return context.WorkflowDefinitionVersions
            .FirstOrDefaultAsync(v => v.Id == id, cancellationToken);
    }

    /// <inheritdoc />
    public Task<WorkflowDefinition?> GetByNameAndCategoryAsync(
        string name,
        string category,
        CancellationToken cancellationToken = default)
    {
        return context.WorkflowDefinitions
            .FirstOrDefaultAsync(
                x => x.Name == name && x.Category == category,
                cancellationToken);
    }
}
