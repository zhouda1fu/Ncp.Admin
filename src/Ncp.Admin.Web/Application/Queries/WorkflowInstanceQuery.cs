using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Infrastructure.Services;
using Ncp.Admin.Web.Application.Services.Workflow;
using NetCorePal.Context;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 流程实例查询DTO
/// </summary>
public record WorkflowInstanceQueryDto(
    WorkflowInstanceId Id,
    WorkflowDefinitionId WorkflowDefinitionId,
    string WorkflowDefinitionName,
    string BusinessKey,
    string BusinessType,
    string Title,
    UserId InitiatorId,
    string InitiatorName,
    WorkflowInstanceStatus Status,
    string CurrentNodeName,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    string Remark);

/// <summary>
/// 流程实例详情查询DTO（包含任务列表）
/// </summary>
public record WorkflowInstanceDetailQueryDto(
    WorkflowInstanceId Id,
    WorkflowDefinitionId WorkflowDefinitionId,
    string WorkflowDefinitionName,
    string BusinessKey,
    string BusinessType,
    string Title,
    UserId InitiatorId,
    string InitiatorName,
    WorkflowInstanceStatus Status,
    string CurrentNodeName,
    string CurrentNodeKey,
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    string Variables,
    IReadOnlyList<WorkflowProgressStepItem> ProgressSteps,
    string Remark,
    IEnumerable<WorkflowTaskQueryDto> Tasks);

/// <summary>
/// 工作流任务查询DTO
/// </summary>
public record WorkflowTaskQueryDto(
    WorkflowTaskId Id,
    WorkflowInstanceId WorkflowInstanceId,
    string NodeKey,
    string NodeName,
    WorkflowTaskType TaskType,
    AssigneeType AssigneeType,
    UserId AssigneeId,
    RoleId AssigneeRoleId,
    string AssigneeName,
    WorkflowTaskStatus Status,
    bool CanOperate,
    string Comment,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);

/// <summary>
/// 我的待办任务查询DTO（包含流程信息）
/// </summary>
public record MyPendingTaskQueryDto(
    WorkflowTaskId TaskId,
    WorkflowInstanceId WorkflowInstanceId,
    string WorkflowTitle,
    string WorkflowDefinitionName,
    string InitiatorName,
    string NodeName,
    WorkflowTaskType TaskType,
    DateTimeOffset CreatedAt);

/// <summary>
/// 我的已办任务查询DTO
/// </summary>
public record MyCompletedTaskQueryDto(
    WorkflowTaskId TaskId,
    WorkflowInstanceId WorkflowInstanceId,
    string WorkflowTitle,
    string WorkflowDefinitionName,
    string InitiatorName,
    string NodeName,
    WorkflowTaskType TaskType,
    WorkflowTaskStatus Status,
    string Comment,
    DateTimeOffset CreatedAt,
    DateTimeOffset? CompletedAt);

/// <summary>
/// 流程实例查询输入
/// </summary>
public class WorkflowInstanceQueryInput : PageRequest
{
    public string? Title { get; set; }
    public string? BusinessType { get; set; }
    public WorkflowInstanceStatus? Status { get; set; }
    public DateTimeOffset? StartTime { get; set; }
    public DateTimeOffset? EndTime { get; set; }
}

/// <summary>
/// 待办任务查询输入
/// </summary>
public class PendingTaskQueryInput : PageRequest
{
    public string? Title { get; set; }
}

/// <summary>
/// 已办任务查询输入
/// </summary>
public class CompletedTaskQueryInput : PageRequest
{
    public string? Title { get; set; }
}

/// <summary>
/// 流程实例查询
/// </summary>
public class WorkflowInstanceQuery(
    ApplicationDbContext applicationDbContext,
    UserQuery userQuery,
    IContextAccessor contextAccessor) : IQuery
{
    private DbSet<WorkflowInstance> InstanceSet { get; } = applicationDbContext.WorkflowInstances;
    private DbSet<WorkflowTask> TaskSet { get; } = applicationDbContext.WorkflowTasks;

    /// <summary>
    /// 是否存在相同 businessType + businessKey 且状态为运行中的流程实例（用于防重复发起）
    /// </summary>
    public async Task<bool> ExistsRunningInstanceByBusinessKeyAsync(
        string businessType,
        string businessKey,
        CancellationToken cancellationToken)
    {
        return await InstanceSet.AsNoTracking()
            .AnyAsync(
                i => i.BusinessType == businessType
                    && i.BusinessKey == businessKey
                    && i.Status == WorkflowInstanceStatus.Running,
                cancellationToken);
    }

    /// <summary>
    /// 获取流程实例列表（分页）
    /// </summary>
    public async Task<PagedData<WorkflowInstanceQueryDto>> GetAllInstancesAsync(
        WorkflowInstanceQueryInput query, CancellationToken cancellationToken)
    {
        return await InstanceSet.AsNoTracking()
            .WhereIf(!string.IsNullOrWhiteSpace(query.Title), i => i.Title.Contains(query.Title!))
            .WhereIf(!string.IsNullOrWhiteSpace(query.BusinessType), i => i.BusinessType == query.BusinessType)
            .WhereIf(query.Status.HasValue, i => i.Status == query.Status)
            .WhereIf(query.StartTime.HasValue, i => i.StartedAt >= query.StartTime!.Value)
            .WhereIf(query.EndTime.HasValue, i => i.StartedAt <= query.EndTime!.Value)
            .OrderByDescending(i => i.StartedAt)
            .Select(i => new WorkflowInstanceQueryDto(
                i.Id,
                i.WorkflowDefinitionId,
                i.WorkflowDefinitionName,
                i.BusinessKey,
                i.BusinessType,
                i.Title,
                i.InitiatorId,
                i.InitiatorName,
                i.Status,
                i.CurrentNodeName,
                i.StartedAt,
                i.CompletedAt,
                i.Remark))
            .ToPagedDataAsync(query, cancellationToken);
    }

    /// <summary>
    /// 获取流程实例详情（包含任务时间线）
    /// </summary>
    public async Task<WorkflowInstanceDetailQueryDto?> GetInstanceDetailAsync(
        WorkflowInstanceId id,
        UserId operatorId,
        CancellationToken cancellationToken)
    {
        var userRoleIds = await userQuery.GetRoleIdsByUserIdAsync(operatorId, cancellationToken);

        var instance = await InstanceSet.AsNoTracking()
            .Include(i => i.Tasks)
            .Where(i => i.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
        if (instance == null)
        {
            return null;
        }

        var definitionJson = await applicationDbContext.WorkflowDefinitions.AsNoTracking()
            .Where(d => d.Id == instance.WorkflowDefinitionId)
            .Select(d => d.DefinitionJson)
            .FirstOrDefaultAsync(cancellationToken);

        var traverser = new WorkflowTreeTraverser();
        var progressSteps = traverser.CollectProgressSteps(definitionJson, instance.Variables);

        var tasks = instance.Tasks
            .OrderBy(t => t.CreatedAt)
            .Select(t => new WorkflowTaskQueryDto(
                t.Id,
                t.WorkflowInstanceId,
                t.NodeKey,
                t.NodeName,
                t.TaskType,
                t.AssigneeType,
                t.AssigneeId,
                t.AssigneeRoleId,
                t.AssigneeName,
                t.Status,
                t.Status == WorkflowTaskStatus.Pending
                && ((t.AssigneeId != new UserId(0) && t.AssigneeId == operatorId)
                    || (t.AssigneeRoleId != new RoleId(Guid.Empty) && userRoleIds.Contains(t.AssigneeRoleId))),
                t.Comment,
                t.CreatedAt,
                t.CompletedAt))
            .ToList();

        return new WorkflowInstanceDetailQueryDto(
            instance.Id,
            instance.WorkflowDefinitionId,
            instance.WorkflowDefinitionName,
            instance.BusinessKey,
            instance.BusinessType,
            instance.Title,
            instance.InitiatorId,
            instance.InitiatorName,
            instance.Status,
            instance.CurrentNodeName,
            instance.CurrentNodeKey,
            instance.StartedAt,
            instance.CompletedAt,
            instance.Variables,
            progressSteps,
            instance.Remark,
            tasks);
    }

    /// <summary>
    /// 获取我发起的流程（分页）
    /// </summary>
    public async Task<PagedData<WorkflowInstanceQueryDto>> GetMyInitiatedWorkflowsAsync(
        UserId initiatorId, WorkflowInstanceQueryInput query, CancellationToken cancellationToken)
    {
        return await InstanceSet.AsNoTracking()
            .Where(i => i.InitiatorId == initiatorId)
            .WhereIf(!string.IsNullOrWhiteSpace(query.Title), i => i.Title.Contains(query.Title!))
            .WhereIf(query.Status.HasValue, i => i.Status == query.Status)
            .OrderByDescending(i => i.StartedAt)
            .Select(i => new WorkflowInstanceQueryDto(
                i.Id,
                i.WorkflowDefinitionId,
                i.WorkflowDefinitionName,
                i.BusinessKey,
                i.BusinessType,
                i.Title,
                i.InitiatorId,
                i.InitiatorName,
                i.Status,
                i.CurrentNodeName,
                i.StartedAt,
                i.CompletedAt,
                i.Remark))
            .ToPagedDataAsync(query, cancellationToken);
    }

    /// <summary>
    /// 获取我的待办任务（分页）
    /// 指定用户：AssigneeId == 当前用户；指定角色：AssigneeRoleId 属于当前用户所属角色
    /// </summary>
    public async Task<PagedData<MyPendingTaskQueryDto>> GetMyPendingTasksAsync(
        UserId assigneeId, PendingTaskQueryInput query, CancellationToken cancellationToken)
    {
        var userRoleIds = await userQuery.GetRoleIdsByUserIdAsync(assigneeId, cancellationToken);

        // “我的待办”按任务归属 + 数据权限交集过滤。
        var baseQuery = from t in TaskSet.AsNoTracking()
                        join i in InstanceSet.AsNoTracking()
                            on t.WorkflowInstanceId equals i.Id
                        where i.Status == WorkflowInstanceStatus.Running
                              && t.Status == WorkflowTaskStatus.Pending
                              && ((t.AssigneeId != new UserId(0) && t.AssigneeId == assigneeId)
                                  || (t.AssigneeRoleId != new RoleId(Guid.Empty) && userRoleIds.Contains(t.AssigneeRoleId)))
                        select new { Instance = i, Task = t };
        var dataPermission = contextAccessor.GetContext<DataPermissionContext>();
        if (dataPermission is { Scope: not DataScope.All })
        {
            switch (dataPermission.Scope)
            {
                case DataScope.Self:
                {
                    if (dataPermission.UserId == null)
                    {
                        baseQuery = baseQuery.Where(_ => false);
                    }
                    else
                    {
                        var currentUserId = dataPermission.UserId;
                        baseQuery = baseQuery.Where(x => x.Instance.InitiatorId == currentUserId);
                    }

                    break;
                }
                case DataScope.Dept:
                {
                    if (dataPermission.DeptId == null)
                    {
                        baseQuery = baseQuery.Where(_ => false);
                    }
                    else
                    {
                        var currentDeptId = dataPermission.DeptId;
                        baseQuery = baseQuery.Where(x => x.Instance.InitiatorDeptId == currentDeptId);
                    }

                    break;
                }
                case DataScope.DeptAndSub:
                case DataScope.CustomDeptAndSub:
                {
                    var deptIds = dataPermission.AuthorizedDeptIds ?? [];
                    baseQuery = deptIds.Count == 0
                        ? baseQuery.Where(_ => false)
                        : baseQuery.Where(x => deptIds.Contains(x.Instance.InitiatorDeptId));
                    break;
                }
            }
        }
        if (!string.IsNullOrWhiteSpace(query.Title))
        {
            baseQuery = baseQuery.Where(x => x.Instance.Title.Contains(query.Title));
        }

        return await baseQuery
            .OrderByDescending(x => x.Task.CreatedAt)
            .Select(x => new MyPendingTaskQueryDto(
                x.Task.Id,
                x.Task.WorkflowInstanceId,
                x.Instance.Title,
                x.Instance.WorkflowDefinitionName,
                x.Instance.InitiatorName,
                x.Task.NodeName,
                x.Task.TaskType,
                x.Task.CreatedAt))
            .ToPagedDataAsync(query, cancellationToken);
    }

    /// <summary>
    /// 获取我的已办任务（分页）
    /// 指定用户：AssigneeId == 当前用户；指定角色：AssigneeRoleId 属于当前用户所属角色
    /// </summary>
    public async Task<PagedData<MyCompletedTaskQueryDto>> GetMyCompletedTasksAsync(
        UserId assigneeId, CompletedTaskQueryInput query, CancellationToken cancellationToken)
    {
        var userRoleIds = await userQuery.GetRoleIdsByUserIdAsync(assigneeId, cancellationToken);

        // “我的已办”同理按任务归属 + 数据权限交集过滤。
        var baseQuery = from t in TaskSet.AsNoTracking()
                        join i in InstanceSet.AsNoTracking()
                            on t.WorkflowInstanceId equals i.Id
                        where t.Status != WorkflowTaskStatus.Pending
                              && ((t.AssigneeId != new UserId(0) && t.AssigneeId == assigneeId)
                                  || (t.AssigneeRoleId != new RoleId(Guid.Empty) && userRoleIds.Contains(t.AssigneeRoleId)))
                        select new { Instance = i, Task = t };
        var dataPermission = contextAccessor.GetContext<DataPermissionContext>();
        if (dataPermission is { Scope: not DataScope.All })
        {
            switch (dataPermission.Scope)
            {
                case DataScope.Self:
                {
                    if (dataPermission.UserId == null)
                    {
                        baseQuery = baseQuery.Where(_ => false);
                    }
                    else
                    {
                        var currentUserId = dataPermission.UserId;
                        baseQuery = baseQuery.Where(x => x.Instance.InitiatorId == currentUserId);
                    }

                    break;
                }
                case DataScope.Dept:
                {
                    if (dataPermission.DeptId == null)
                    {
                        baseQuery = baseQuery.Where(_ => false);
                    }
                    else
                    {
                        var currentDeptId = dataPermission.DeptId;
                        baseQuery = baseQuery.Where(x => x.Instance.InitiatorDeptId == currentDeptId);
                    }

                    break;
                }
                case DataScope.DeptAndSub:
                case DataScope.CustomDeptAndSub:
                {
                    var deptIds = dataPermission.AuthorizedDeptIds ?? [];
                    baseQuery = deptIds.Count == 0
                        ? baseQuery.Where(_ => false)
                        : baseQuery.Where(x => deptIds.Contains(x.Instance.InitiatorDeptId));
                    break;
                }
            }
        }
        if (!string.IsNullOrWhiteSpace(query.Title))
        {
            baseQuery = baseQuery.Where(x => x.Instance.Title.Contains(query.Title));
        }

        return await baseQuery
            .OrderByDescending(x => x.Task.CompletedAt)
            .Select(x => new MyCompletedTaskQueryDto(
                x.Task.Id,
                x.Task.WorkflowInstanceId,
                x.Instance.Title,
                x.Instance.WorkflowDefinitionName,
                x.Instance.InitiatorName,
                x.Task.NodeName,
                x.Task.TaskType,
                x.Task.Status,
                x.Task.Comment,
                x.Task.CreatedAt,
                x.Task.CompletedAt))
            .ToPagedDataAsync(query, cancellationToken);
    }
}
