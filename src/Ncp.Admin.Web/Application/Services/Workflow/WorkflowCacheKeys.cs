using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;

namespace Ncp.Admin.Web.Application.Services.Workflow;

/// <summary>
/// 工作流相关缓存键，供 Query 与 Command 层统一使用
/// </summary>
public static class WorkflowCacheKeys
{
    /// <summary>
    /// 流程定义详情缓存键前缀，完整键为 workflow_definition:{id}
    /// </summary>
    public const string DefinitionKeyPrefix = "workflow_definition:";

    /// <summary>
    /// 已发布流程定义列表缓存键
    /// </summary>
    public const string PublishedListKey = "workflow_definitions:published";

    /// <summary>
    /// 获取流程定义详情缓存键
    /// </summary>
    public static string DefinitionKey(WorkflowDefinitionId id) => $"{DefinitionKeyPrefix}{id}";
}
