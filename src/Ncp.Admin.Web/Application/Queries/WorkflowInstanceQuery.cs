using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Infrastructure.Services;
using Ncp.Admin.Web.Application.Services.Workflow;
using Ncp.Admin.Web.Application.Services.Workflow.Graph;
using NetCorePal.Context;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 流程实例查询DTO
/// </summary>
public record WorkflowInstanceQueryDto(
    WorkflowInstanceId Id,
    WorkflowDefinitionId WorkflowDefinitionId,
    string WorkflowDefinitionName,
    string WorkflowDefinitionCategory,
    string BusinessKey,
    string BusinessType,
    string Title,
    UserId InitiatorId,
    string InitiatorName,
    WorkflowInstanceStatus Status,
    string CurrentNodeName,
    DateTimeOffset StartedAt,
    DateTimeOffset DueAt,
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
    DateTimeOffset? SuspendedAt,
    DateTimeOffset? ResumedAt,
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
    DateTimeOffset? CompletedAt,
    UserId CompletedByUserId,
    string? CompletedByUserDisplayName,
    string ActorDeptName,
    string ActorRoleNames,
    WorkflowTaskReturnContextDto? ReturnContext);

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
    public string? Keyword { get; set; }
    public string? Title { get; set; }
    public string? BusinessType { get; set; }
    public WorkflowDefinitionId? WorkflowDefinitionId { get; set; }
    public string? Category { get; set; }
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
    IContextAccessor contextAccessor,
    WorkflowGraphRuntimeService graphRuntimeService,
    IWorkflowVisibilityService workflowVisibilityService) : IQuery
{
    private DbSet<WorkflowInstance> InstanceSet { get; } = applicationDbContext.WorkflowInstances;
    private DbSet<WorkflowTask> TaskSet { get; } = applicationDbContext.WorkflowTasks;
    private DbSet<WorkflowTaskAssignmentSnapshot> SnapshotSet { get; } = applicationDbContext.WorkflowTaskAssignmentSnapshots;

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
            .ApplyWorkflowInstanceFilters(applicationDbContext, query)
            .OrderByDescending(i => i.StartedAt)
            .Select(i => new WorkflowInstanceQueryDto(
                i.Id,
                i.WorkflowDefinitionId,
                i.WorkflowDefinitionName,
                applicationDbContext.WorkflowDefinitions
                    .Where(d => d.Id == i.WorkflowDefinitionId)
                    .Select(d => d.Category)
                    .FirstOrDefault() ?? string.Empty,
                i.BusinessKey,
                i.BusinessType,
                i.Title,
                i.InitiatorId,
                i.InitiatorName,
                i.Status,
                i.CurrentNodeName,
                i.StartedAt,
                i.StartedAt.AddDays(3),
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

        var instance = await applicationDbContext.WorkflowInstances.AsNoTracking()
            .IgnoreQueryFilters()
            .Include(i => i.Tasks)
            .Where(i => i.Id == id)
            .FirstOrDefaultAsync(cancellationToken);
        if (instance == null)
        {
            return null;
        }

        var taskIds = instance.Tasks.Select(t => t.Id).ToList();
        var snapshots = await SnapshotSet.AsNoTracking()
            .Where(s => taskIds.Contains(s.WorkflowTaskId))
            .ToListAsync(cancellationToken);
        var snapshotsByTaskId = snapshots
            .GroupBy(s => s.WorkflowTaskId)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<WorkflowTaskAssignmentSnapshot>)g.ToList());

        if (!await workflowVisibilityService.CanViewInstanceDetailAsync(
                instance,
                snapshotsByTaskId,
                operatorId,
                userRoleIds,
                cancellationToken))
        {
            return null;
        }

        var graphSnapshotJson = await applicationDbContext.WorkflowDefinitionVersions.AsNoTracking()
            .Where(v => v.Id == instance.WorkflowDefinitionVersionId)
            .Select(v => v.GraphSnapshotJson)
            .FirstOrDefaultAsync(cancellationToken);

        var progressSteps = graphRuntimeService.CollectProgressSteps(graphSnapshotJson, instance.Variables);

        var orderedTasks = instance.Tasks.OrderBy(t => t.CreatedAt).ToList();
        var completedByIds = orderedTasks
            .Select(t => t.CompletedByUserId)
            .Where(id => id != UserId.Unassigned)
            .Distinct()
            .ToList();

        var completedByDisplayName = new Dictionary<UserId, string>();
        foreach (var uid in completedByIds)
        {
            var u = await userQuery.GetUserByIdAsync(uid, cancellationToken);
            completedByDisplayName[uid] = u == null
                ? uid.ToString()
                : (!string.IsNullOrWhiteSpace(u.RealName) ? u.RealName : u.Name);
        }

        var actorUserIds = orderedTasks
            .SelectMany(t =>
            {
                if (t.CompletedByUserId != UserId.Unassigned)
                    return new[] { t.CompletedByUserId, t.AssigneeId };
                return new[] { t.AssigneeId };
            })
            .Distinct()
            .ToList();
        var userDeptPositions = await userQuery.GetUserDeptPositionsByIdsAsync(
            actorUserIds,
            cancellationToken);

        var tasks = orderedTasks
            .Select(t =>
            {
                var actorUserId = t.CompletedByUserId != UserId.Unassigned
                    ? t.CompletedByUserId
                    : t.AssigneeId;
                var actorOrg = userDeptPositions.GetValueOrDefault(actorUserId);
                return new WorkflowTaskQueryDto(
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
                workflowVisibilityService.CanOperateTask(t, snapshotsByTaskId, operatorId, userRoleIds),
                t.Comment,
                t.CreatedAt,
                t.CompletedAt,
                t.CompletedByUserId,
                t.CompletedByUserId != UserId.Unassigned
                    ? completedByDisplayName.GetValueOrDefault(t.CompletedByUserId)
                    : null,
                actorOrg?.DeptName ?? string.Empty,
                actorOrg?.RoleNames ?? string.Empty,
                WorkflowTaskExtraData.TryReadReturnContext(t.ExtraDataJson));
            })
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
            instance.CompletedAt == DateTimeOffset.MinValue ? null : instance.CompletedAt,
            instance.SuspendedAt,
            instance.ResumedAt,
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
            .ApplyWorkflowInstanceFilters(applicationDbContext, query)
            .OrderByDescending(i => i.StartedAt)
            .Select(i => new WorkflowInstanceQueryDto(
                i.Id,
                i.WorkflowDefinitionId,
                i.WorkflowDefinitionName,
                applicationDbContext.WorkflowDefinitions
                    .Where(d => d.Id == i.WorkflowDefinitionId)
                    .Select(d => d.Category)
                    .FirstOrDefault() ?? string.Empty,
                i.BusinessKey,
                i.BusinessType,
                i.Title,
                i.InitiatorId,
                i.InitiatorName,
                i.Status,
                i.CurrentNodeName,
                i.StartedAt,
                i.StartedAt.AddDays(3),
                i.CompletedAt,
                i.Remark))
            .ToPagedDataAsync(query, cancellationToken);
    }

    /// <summary>
    /// 获取我的待办任务（分页）
    /// 指定用户：授权快照用户 == 当前用户；指定角色：授权快照角色属于当前用户所属角色。
    /// 数据权限：仅约束按角色生成的授权快照；指名到人的节点不受发起人数据范围限制。
    /// </summary>
    public async Task<PagedData<MyPendingTaskQueryDto>> GetMyPendingTasksAsync(
        UserId assigneeId, PendingTaskQueryInput query, CancellationToken cancellationToken)
    {
        var userRoleIds = await userQuery.GetRoleIdsByUserIdAsync(assigneeId, cancellationToken);

        // 「我的待办」从授权快照读取处理权限，避免角色和部门配置变动影响已创建任务的归属。
        var baseQuery = from t in TaskSet.AsNoTracking()
                        join s in SnapshotSet.AsNoTracking()
                            on t.Id equals s.WorkflowTaskId
                        join i in applicationDbContext.WorkflowInstances.AsNoTracking().IgnoreQueryFilters()
                            on t.WorkflowInstanceId equals i.Id
                        where i.Status == WorkflowInstanceStatus.Running
                              && t.Status == WorkflowTaskStatus.Pending
                              && ((s.AssigneeType == AssigneeType.User && s.AssigneeUserId == assigneeId)
                                  || (s.AssigneeType == AssigneeType.Role && userRoleIds.Contains(s.AssigneeRoleId)))
                        select new WorkflowTaskSnapshotProjection
                        {
                            Instance = i,
                            Task = t,
                            Snapshot = s
                        };
        var dataPermission = contextAccessor.GetContext<DataPermissionContext>();
        baseQuery = workflowVisibilityService.ApplyTaskDisplayFilter(baseQuery, dataPermission, assigneeId, userRoleIds);
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
    /// 指定用户：授权快照用户 == 当前用户；指定角色：授权快照角色属于当前用户所属角色。
    /// 数据权限与「我的待办」一致。
    /// </summary>
    public async Task<PagedData<MyCompletedTaskQueryDto>> GetMyCompletedTasksAsync(
        UserId assigneeId, CompletedTaskQueryInput query, CancellationToken cancellationToken)
    {
        var userRoleIds = await userQuery.GetRoleIdsByUserIdAsync(assigneeId, cancellationToken);

        // 「我的已办」与待办相同，从授权快照读取任务归属。
        var baseQuery = from t in TaskSet.AsNoTracking()
                        join s in SnapshotSet.AsNoTracking()
                            on t.Id equals s.WorkflowTaskId
                        join i in applicationDbContext.WorkflowInstances.AsNoTracking().IgnoreQueryFilters()
                            on t.WorkflowInstanceId equals i.Id
                        where t.Status != WorkflowTaskStatus.Pending
                              && ((s.AssigneeType == AssigneeType.User && s.AssigneeUserId == assigneeId)
                                  || (s.AssigneeType == AssigneeType.Role && userRoleIds.Contains(s.AssigneeRoleId)))
                        select new WorkflowTaskSnapshotProjection
                        {
                            Instance = i,
                            Task = t,
                            Snapshot = s
                        };
        var dataPermission = contextAccessor.GetContext<DataPermissionContext>();
        baseQuery = workflowVisibilityService.ApplyTaskDisplayFilter(baseQuery, dataPermission, assigneeId, userRoleIds);
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

public sealed class WorkflowTaskSnapshotProjection
{
    public WorkflowInstance Instance { get; init; } = null!;
    public WorkflowTask Task { get; init; } = null!;
    public WorkflowTaskAssignmentSnapshot Snapshot { get; init; } = null!;
}

internal static class WorkflowInstanceQueryableExtensions
{
    public static IQueryable<WorkflowInstance> ApplyWorkflowInstanceFilters(
        this IQueryable<WorkflowInstance> source,
        ApplicationDbContext dbContext,
        WorkflowInstanceQueryInput query)
    {
        var keyword = !string.IsNullOrWhiteSpace(query.Keyword) ? query.Keyword : query.Title;
        source = source
            .WhereIf(!string.IsNullOrWhiteSpace(keyword),
                i => i.Title.Contains(keyword!) || i.BusinessKey.Contains(keyword!))
            .WhereIf(!string.IsNullOrWhiteSpace(query.BusinessType), i => i.BusinessType == query.BusinessType)
            .WhereIf(query.WorkflowDefinitionId != null, i => i.WorkflowDefinitionId == query.WorkflowDefinitionId)
            .WhereIf(query.Status.HasValue, i => i.Status == query.Status)
            .WhereIf(query.StartTime.HasValue, i => i.StartedAt >= query.StartTime!.Value)
            .WhereIf(query.EndTime.HasValue, i => i.StartedAt <= query.EndTime!.Value);

        if (!string.IsNullOrWhiteSpace(query.Category))
        {
            source = source.Where(i => dbContext.WorkflowDefinitions
                .Any(d => d.Id == i.WorkflowDefinitionId && d.Category == query.Category));
        }

        return source;
    }
}
