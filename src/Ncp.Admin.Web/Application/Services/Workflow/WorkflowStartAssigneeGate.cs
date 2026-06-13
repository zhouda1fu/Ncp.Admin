using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.Application.Services.Workflow.Graph;

namespace Ncp.Admin.Web.Application.Services.Workflow;

/// <summary>
/// 在持久化业务数据前，按与 <see cref="Commands.Workflows.StartWorkflowCommandHandler"/> 相同的规则
/// 校验首个审批节点在数据权限过滤后是否仍有处理人，便于保存时返回明确错误码（如新建办公任务）。
/// </summary>
public class WorkflowStartAssigneeGate(
    WorkflowGraphRuntimeService graphRuntimeService,
    IWorkflowApprovalAssignmentService approvalAssignmentService)
{
    /// <summary>
    /// 模拟发起流程时对首个审批节点的解析；若数据权限过滤后无人可选则抛出 <see cref="ErrorCodes.WorkflowAssigneeDataPermissionDenied"/> 等。
    /// </summary>
    public async Task EnsureFirstApprovalResolvableAsync(
        WorkflowDefinitionId definitionId,
        string workflowDefinitionName,
        string businessType,
        string title,
        string graphSnapshotJson,
        string? variables,
        UserId initiatorId,
        string initiatorName,
        DeptId initiatorDeptId,
        CancellationToken cancellationToken = default)
    {
        RequireGraphSnapshotJson(graphSnapshotJson);

        var instance = new WorkflowInstance(
            definitionId,
            WorkflowDefinitionVersionId.Unassigned,
            workflowDefinitionName,
            string.Empty,
            businessType,
            title,
            initiatorId,
            initiatorName,
            initiatorDeptId,
            variables ?? "{}",
            string.Empty);

        var node = graphRuntimeService.FindFirstTaskNode(graphSnapshotJson, variables);
        while (node != null)
        {
            if (IsOfficeTaskParticipantConfigNode(node))
            {
                node = graphRuntimeService.FindNextTaskNode(graphSnapshotJson, node.NodeId, variables);
                continue;
            }

            var resolution = await approvalAssignmentService.ResolveForTaskCreationAsync(
                node,
                instance,
                graphSnapshotJson,
                cancellationToken);

            if (IsFirstHumanApprovalNode(node, resolution))
            {
                return;
            }

            node = graphRuntimeService.FindNextTaskNode(graphSnapshotJson, node.NodeId, variables);
        }
    }

    /// <summary>
    /// 解析流程中首个「需人工审批」节点（与发起前校验逻辑一致；跳过办公任务主接收人/抄送人配置节点）的处理人列表。
    /// </summary>
    public async Task<IReadOnlyList<WorkflowAssigneeResult>> ResolveFirstApprovalAssigneesAsync(
        string graphSnapshotJson,
        string? variables,
        WorkflowInstance instance,
        CancellationToken cancellationToken = default)
    {
        RequireGraphSnapshotJson(graphSnapshotJson);

        var node = graphRuntimeService.FindFirstTaskNode(graphSnapshotJson, variables);
        while (node != null)
        {
            if (IsOfficeTaskParticipantConfigNode(node))
            {
                node = graphRuntimeService.FindNextTaskNode(graphSnapshotJson, node.NodeId, variables);
                continue;
            }

            var resolution = await approvalAssignmentService.ResolveForTaskCreationAsync(
                node,
                instance,
                graphSnapshotJson,
                cancellationToken);

            if (IsFirstHumanApprovalNode(node, resolution))
            {
                return resolution.Assignees
                    .Where(a => a.AssigneeId != UserId.Unassigned)
                    .ToList();
            }

            node = graphRuntimeService.FindNextTaskNode(graphSnapshotJson, node.NodeId, variables);
        }

        return [];
    }

    private static void RequireGraphSnapshotJson(string? graphSnapshotJson)
    {
        if (string.IsNullOrWhiteSpace(graphSnapshotJson))
        {
            throw new KnownException("流程缺少已发布的运行图快照", ErrorCodes.WorkflowDefinitionNotFound);
        }
    }

    /// <summary>
    /// 办公任务流程中「标识为主接收人/抄送人」的审批节点不算业务上的首个审核人。
    /// </summary>
    private static bool IsFirstHumanApprovalNode(WorkflowGraphNode node, WorkflowAssigneeResolutionResult resolution) =>
        node.Type == WorkflowGraphNodeType.Approval && !resolution.AutoPassed && !node.OfficeTaskParticipantNode();

    /// <summary>
    /// 办公任务「主接收人/抄送人配置」节点：仅用于新建页解析参与人，不应生成工作流待办。
    /// </summary>
    public static bool IsOfficeTaskParticipantConfigNode(WorkflowGraphNode node) =>
        node.Type == WorkflowGraphNodeType.Approval && node.OfficeTaskParticipantNode();
}
