using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;

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
    DateTimeOffset StartedAt,
    DateTimeOffset? CompletedAt,
    string Variables,
    string Remark,
    IEnumerable<WorkflowTaskQueryDto> Tasks);

/// <summary>
/// 工作流任务查询DTO
/// </summary>
public record WorkflowTaskQueryDto(
    WorkflowTaskId Id,
    WorkflowInstanceId WorkflowInstanceId,
    string NodeName,
    WorkflowTaskType TaskType,
    UserId AssigneeId,
    string AssigneeName,
    WorkflowTaskStatus Status,
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
public class WorkflowInstanceQuery(ApplicationDbContext applicationDbContext) : IQuery
{
    private DbSet<WorkflowInstance> InstanceSet { get; } = applicationDbContext.WorkflowInstances;
    private DbSet<WorkflowTask> TaskSet { get; } = applicationDbContext.WorkflowTasks;

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
        WorkflowInstanceId id, CancellationToken cancellationToken)
    {
        return await InstanceSet.AsNoTracking()
            .Where(i => i.Id == id)
            .Select(i => new WorkflowInstanceDetailQueryDto(
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
                i.Variables,
                i.Remark,
                i.Tasks.OrderBy(t => t.CreatedAt).Select(t => new WorkflowTaskQueryDto(
                    t.Id,
                    t.WorkflowInstanceId,
                    t.NodeName,
                    t.TaskType,
                    t.AssigneeId,
                    t.AssigneeName,
                    t.Status,
                    t.Comment,
                    t.CreatedAt,
                    t.CompletedAt))))
            .FirstOrDefaultAsync(cancellationToken);
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
    /// 使用 LINQ 查询语法，先在实体层面排序再投影，避免 EF Core 翻译问题
    /// </summary>
    public async Task<PagedData<MyPendingTaskQueryDto>> GetMyPendingTasksAsync(
        UserId assigneeId, PendingTaskQueryInput query, CancellationToken cancellationToken)
    {
        var baseQuery = from i in InstanceSet.AsNoTracking()
                        from t in i.Tasks
                        where t.AssigneeId == assigneeId && t.Status == WorkflowTaskStatus.Pending
                        select new { Instance = i, Task = t };

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
    /// 使用 LINQ 查询语法，先在实体层面排序再投影，避免 EF Core 翻译问题
    /// </summary>
    public async Task<PagedData<MyCompletedTaskQueryDto>> GetMyCompletedTasksAsync(
        UserId assigneeId, CompletedTaskQueryInput query, CancellationToken cancellationToken)
    {
        var baseQuery = from i in InstanceSet.AsNoTracking()
                        from t in i.Tasks
                        where t.AssigneeId == assigneeId && t.Status != WorkflowTaskStatus.Pending
                        select new { Instance = i, Task = t };

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
