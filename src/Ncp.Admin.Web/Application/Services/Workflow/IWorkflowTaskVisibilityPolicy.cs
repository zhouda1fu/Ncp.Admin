using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Application.Services.Workflow;

/// <summary>
/// 工作流任务可见性策略。用于在任务创建和通知发送前应用数据权限边界。
/// </summary>
public interface IWorkflowTaskVisibilityPolicy
{
    /// <summary>
    /// 过滤候选审批人，只保留其数据权限能够覆盖流程发起人的处理人。
    /// </summary>
    Task<IReadOnlyList<WorkflowAssigneeResult>> FilterAssigneesByDataPermissionAsync(
        WorkflowInstance instance,
        IReadOnlyList<WorkflowAssigneeResult> assignees,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 判断用户是否能基于数据权限看到指定发起人范围内的流程。
    /// </summary>
    Task<bool> CanUserAccessWorkflowByDataPermissionAsync(
        UserId userId,
        UserId initiatorId,
        DeptId initiatorDeptId,
        CancellationToken cancellationToken = default);
}
