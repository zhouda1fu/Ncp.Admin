using Ncp.Admin.Domain.AggregatesModel.NotificationAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Domain.DomainEvents;
using Ncp.Admin.Web.Application.Commands.Notifications;
using Ncp.Admin.Web.Application.Commands.Workflows;

namespace Ncp.Admin.Web.Application.DomainEventHandlers;
/// <summary>
/// 工作流任务创建领域事件处理器：向任务处理人发送待办/抄送/通知消息
/// </summary>
public class WorkflowTaskCreatedDomainEventHandlerForSendNotification(IMediator mediator)
    : IDomainEventHandler<WorkflowTaskCreatedDomainEvent>
{
    public async Task Handle(WorkflowTaskCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var instance = domainEvent.WorkflowInstance;
        var task = domainEvent.WorkflowTask;

        // 角色池任务 AssigneeId 为 0，此处不推送（无单一接收人）；指名到人的待办一律通知，不按发起人数据范围拦截。
        if (task.AssigneeId == UserId.Unassigned)
            return;

        var receiverId = task.AssigneeId.Id;
        var (title, content) = task.TaskType switch
        {
            WorkflowTaskType.CarbonCopy => (
                "流程抄送提醒",
                $"流程「{instance.Title}」已抄送给您，请查阅（无需审批）。"),
            WorkflowTaskType.Notification => (
                "流程通知",
                $"流程「{instance.Title}」有一条通知，请查阅。"),
            _ => (
                "您有一条待办审批",
                $"流程「{instance.Title}」需要您审批，请及时处理。"),
        };

        await mediator.Send(
            new SendNotificationCommand(
                title,
                content,
                NotificationType.Workflow,
                NotificationLevel.Info,
                null,
                instance.InitiatorName,
                receiverId,
                instance.Id.ToString(),
                "WorkflowInstance"),
            cancellationToken);
    }
}
