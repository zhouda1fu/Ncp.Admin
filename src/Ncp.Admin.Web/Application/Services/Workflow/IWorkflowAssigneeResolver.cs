using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.Application.Services.Workflow.Graph;

namespace Ncp.Admin.Web.Application.Services.Workflow;

/// <summary>
/// 工作流审批人解析抽象，便于服务层测试与替换解析策略。
/// </summary>
public interface IWorkflowAssigneeResolver
{
    /// <summary>
    /// 解析节点的首个可用处理人。
    /// </summary>
    Task<WorkflowAssigneeResult?> ResolveAssigneeAsync(
        WorkflowGraphNode node,
        WorkflowInstance instance,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 解析节点配置中的候选处理人列表，返回顺序需保持设计器配置或业务解析顺序。
    /// </summary>
    Task<IReadOnlyList<WorkflowAssigneeResult>> ResolveAssigneesAsync(
        WorkflowGraphNode node,
        WorkflowInstance instance,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 解析用于任务创建的有序处理人列表。依次审批会按该顺序逐个创建任务。
    /// </summary>
    Task<IReadOnlyList<WorkflowAssigneeResult>> ResolveOrderedAssigneesAsync(
        WorkflowGraphNode node,
        WorkflowInstance instance,
        CancellationToken cancellationToken = default);
}
