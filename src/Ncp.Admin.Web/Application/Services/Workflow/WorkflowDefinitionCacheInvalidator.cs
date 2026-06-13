using Microsoft.Extensions.Caching.Memory;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;

namespace Ncp.Admin.Web.Application.Services.Workflow;

/// <summary>
/// 工作流定义缓存失效入口。所有定义写操作都从这里清理缓存，避免命令层散落缓存 key。
/// </summary>
public class WorkflowDefinitionCacheInvalidator(IMemoryCache memoryCache)
{
    /// <summary>
    /// 清理单条流程定义详情缓存。
    /// </summary>
    public void InvalidateDefinition(WorkflowDefinitionId id)
    {
        memoryCache.Remove(WorkflowCacheKeys.DefinitionKey(id));
    }

    /// <summary>
    /// 清理单条流程定义和已发布定义列表缓存，适用于更新、删除、发布等写操作。
    /// </summary>
    public void InvalidateDefinitionWrite(WorkflowDefinitionId id)
    {
        InvalidateDefinition(id);
        InvalidatePublishedList();
    }

    /// <summary>
    /// 批量清理多条流程定义详情，并清理已发布定义列表缓存。
    /// </summary>
    public void InvalidateDefinitionWrite(params WorkflowDefinitionId[] ids)
    {
        foreach (var id in ids.Distinct())
        {
            InvalidateDefinition(id);
        }

        InvalidatePublishedList();
    }

    /// <summary>
    /// 清理已发布定义列表缓存。
    /// </summary>
    public void InvalidatePublishedList()
    {
        memoryCache.Remove(WorkflowCacheKeys.PublishedListKey);
    }
}
