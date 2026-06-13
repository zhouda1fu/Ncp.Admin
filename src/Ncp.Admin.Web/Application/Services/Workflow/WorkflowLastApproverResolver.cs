using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;

namespace Ncp.Admin.Web.Application.Services.Workflow;

/// <summary>
/// 解析工作流实例中最后一位完成审批的处理人。
/// </summary>
public static class WorkflowLastApproverResolver
{
    private static readonly UserId EmptyUserId = UserId.Unassigned;

    public static (UserId UserId, string DisplayName) Resolve(WorkflowInstance instance)
    {
        var lastTask = instance.Tasks
            .Where(t =>
                t.TaskType == WorkflowTaskType.Approval
                && t.Status == WorkflowTaskStatus.Approved
                && t.CompletedAt > DateTimeOffset.MinValue)
            .OrderByDescending(t => t.CompletedAt)
            .FirstOrDefault();

        if (lastTask is null)
        {
            return (EmptyUserId, string.Empty);
        }

        var userId = lastTask.CompletedByUserId != EmptyUserId
            ? lastTask.CompletedByUserId
            : lastTask.AssigneeType == AssigneeType.User
                ? lastTask.AssigneeId
                : EmptyUserId;

        var name = !string.IsNullOrWhiteSpace(lastTask.AssigneeName)
            ? lastTask.AssigneeName.Trim()
            : string.Empty;

        return (userId, name);
    }
}
