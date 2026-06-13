using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Infrastructure;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Application.Services.Notification;

/// <summary>
/// 按接收人解析通知跳转路径（平台精简：工作流实例 + 系统通知）。
/// </summary>
public class NotificationNavigationResolver(ApplicationDbContext db, UserQuery userQuery)
{
    public static string ToCacheKey(string? businessType, string? businessId) =>
        $"{businessType}\0{businessId ?? string.Empty}";

    public async Task<NotificationNavigationResult> ResolveAsync(
        UserId userId,
        string? businessId,
        string? businessType,
        CancellationToken cancellationToken = default)
    {
        var batch = await ResolveBatchAsync(
            userId,
            [(businessType, businessId, null)],
            cancellationToken);
        return batch.GetValueOrDefault(ToCacheKey(businessType, businessId))
               ?? NotificationRoutePaths.FromBusinessType(businessType ?? string.Empty, businessId);
    }

    public async Task<IReadOnlyDictionary<string, NotificationNavigationResult>> ResolveBatchAsync(
        UserId userId,
        IEnumerable<(string? BusinessType, string? BusinessId, long? NotificationSenderId)> items,
        CancellationToken cancellationToken = default)
    {
        var distinct = items
            .Where(x => !string.IsNullOrWhiteSpace(x.BusinessType))
            .DistinctBy(x => ToCacheKey(x.BusinessType, x.BusinessId))
            .ToList();

        if (distinct.Count == 0)
        {
            return new Dictionary<string, NotificationNavigationResult>(StringComparer.Ordinal);
        }

        var userRoleIds = await userQuery.GetRoleIdsByUserIdAsync(userId, cancellationToken);
        var workflowInstanceIds = new HashSet<WorkflowInstanceId>();

        foreach (var (businessType, businessId, _) in distinct)
        {
            if (string.Equals(businessType, "WorkflowInstance", StringComparison.Ordinal)
                && TryParseWorkflowInstanceId(businessId, out var instanceId))
            {
                workflowInstanceIds.Add(instanceId);
            }
        }

        var workflowTasksByInstance = await LoadWorkflowTasksByInstanceAsync(
            userId,
            userRoleIds,
            workflowInstanceIds,
            cancellationToken);

        var result = new Dictionary<string, NotificationNavigationResult>(StringComparer.Ordinal);
        foreach (var (businessType, businessId, _) in distinct)
        {
            var navigation = businessType switch
            {
                "WorkflowInstance" => ResolveWorkflowInstance(userId, businessId, workflowTasksByInstance),
                _ => NotificationRoutePaths.FromBusinessType(businessType!, businessId),
            };
            result[ToCacheKey(businessType, businessId)] = navigation;
        }

        return result;
    }

    private static NotificationNavigationResult ResolveWorkflowInstance(
        UserId userId,
        string? businessId,
        IReadOnlyDictionary<WorkflowInstanceId, List<WorkflowTaskNavigationRow>> workflowTasksByInstance)
    {
        if (!TryParseWorkflowInstanceId(businessId, out var instanceId))
        {
            return new("/workflow/pending");
        }

        workflowTasksByInstance.TryGetValue(instanceId, out var myTasks);
        myTasks ??= [];

        var hasPendingApproval = myTasks.Any(t =>
            t.Status == WorkflowTaskStatus.Pending && t.TaskType == WorkflowTaskType.Approval);

        return hasPendingApproval
            ? NotificationRoutePaths.WorkflowInstance(businessId!)
            : new("/workflow/pending");
    }

    private async Task<IReadOnlyDictionary<WorkflowInstanceId, List<WorkflowTaskNavigationRow>>> LoadWorkflowTasksByInstanceAsync(
        UserId userId,
        IReadOnlyList<RoleId> userRoleIds,
        IReadOnlySet<WorkflowInstanceId> workflowInstanceIds,
        CancellationToken cancellationToken)
    {
        if (workflowInstanceIds.Count == 0)
        {
            return new Dictionary<WorkflowInstanceId, List<WorkflowTaskNavigationRow>>();
        }

        var rows = await (
                from t in db.WorkflowTasks.AsNoTracking()
                join s in db.WorkflowTaskAssignmentSnapshots.AsNoTracking()
                    on t.Id equals s.WorkflowTaskId
                where workflowInstanceIds.Contains(t.WorkflowInstanceId)
                      && ((s.AssigneeType == AssigneeType.User && s.AssigneeUserId == userId)
                          || (s.AssigneeType == AssigneeType.Role && userRoleIds.Contains(s.AssigneeRoleId)))
                select new WorkflowTaskNavigationRow(t.WorkflowInstanceId, t.Status, t.TaskType))
            .ToListAsync(cancellationToken);

        return rows
            .GroupBy(x => x.WorkflowInstanceId)
            .ToDictionary(g => g.Key, g => g.ToList());
    }

    private static bool TryParseWorkflowInstanceId(string? businessId, out WorkflowInstanceId instanceId)
    {
        if (string.IsNullOrWhiteSpace(businessId)
            || !Guid.TryParse(businessId, out var guid)
            || guid == Guid.Empty)
        {
            instanceId = WorkflowInstanceId.Unassigned;
            return false;
        }

        instanceId = new WorkflowInstanceId(guid);
        return true;
    }

    private sealed record WorkflowTaskNavigationRow(
        WorkflowInstanceId WorkflowInstanceId,
        WorkflowTaskStatus Status,
        WorkflowTaskType TaskType);
}
