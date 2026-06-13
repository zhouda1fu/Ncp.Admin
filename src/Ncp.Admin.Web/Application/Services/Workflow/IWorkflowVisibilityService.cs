using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Infrastructure.Services;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Application.Services.Workflow;

/// <summary>
/// 工作流可见性服务。显式区分任务展示、任务操作和实例详情访问，避免依赖 EF 全局过滤的隐式规则。
/// </summary>
public interface IWorkflowVisibilityService
{
    /// <summary>
    /// 按任务授权快照和数据权限过滤“我的任务”列表。
    /// </summary>
    IQueryable<WorkflowTaskSnapshotProjection> ApplyTaskDisplayFilter(
        IQueryable<WorkflowTaskSnapshotProjection> query,
        DataPermissionContext? dataPermission,
        UserId assigneeId,
        IReadOnlyCollection<RoleId> userRoleIds);

    /// <summary>
    /// 判断当前用户是否可查看实例详情。管理数据范围命中或工作流任务授权命中任一即可查看。
    /// </summary>
    Task<bool> CanViewInstanceDetailAsync(
        WorkflowInstance instance,
        IReadOnlyDictionary<WorkflowTaskId, IReadOnlyList<WorkflowTaskAssignmentSnapshot>> snapshotsByTaskId,
        UserId operatorId,
        IReadOnlyCollection<RoleId> operatorRoleIds,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 判断当前用户是否可操作指定待办任务。
    /// </summary>
    bool CanOperateTask(
        WorkflowTask task,
        IReadOnlyDictionary<WorkflowTaskId, IReadOnlyList<WorkflowTaskAssignmentSnapshot>> snapshotsByTaskId,
        UserId operatorId,
        IReadOnlyCollection<RoleId> operatorRoleIds);
}
