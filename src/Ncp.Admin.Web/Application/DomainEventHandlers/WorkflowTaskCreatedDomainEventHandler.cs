using Ncp.Admin.Domain.AggregatesModel.NotificationAggregate;
using Ncp.Admin.Domain.DomainEvents.WorkflowEvents;
using Ncp.Admin.Web.Application.Commands.Notification;
using Serilog;

namespace Ncp.Admin.Web.Application.DomainEventHandlers;

/// <summary>
/// 工作流任务创建领域事件处理器：向审批人发送待办通知
/// </summary>
public class WorkflowTaskCreatedDomainEventHandler(IMediator mediator) : IDomainEventHandler<WorkflowTaskCreatedDomainEvent>
{
    public async Task Handle(WorkflowTaskCreatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var instance = domainEvent.WorkflowInstance;
        var task = domainEvent.WorkflowTask;

        // 仅对指定用户的任务发送通知（角色任务需解析用户，暂不实现）
        if (task.AssigneeId == null)
            return;

        var receiverId = task.AssigneeId!.Id;
        var title = "您有一条待办审批";
        var content = $"流程「{instance.Title}」需要您审批，请及时处理。";

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

        Log.Debug("已向审批人发送待办通知: InstanceId={InstanceId}, AssigneeId={AssigneeId}",
            instance.Id, receiverId);
    }
}
