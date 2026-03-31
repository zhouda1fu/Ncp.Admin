using Ncp.Admin.Domain.AggregatesModel.NotificationAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Domain.DomainEvents.WorkflowEvents;
using Ncp.Admin.Web.Application.Commands.Notification;

namespace Ncp.Admin.Web.Application.DomainEventHandlers;

/// <summary>
/// 工作流实例完成领域事件处理器：向发起人发送审批通过通知
/// </summary>
public class WorkflowInstanceCompletedDomainEventHandlerForSendNotification(IMediator mediator)
    : IDomainEventHandler<WorkflowInstanceCompletedDomainEvent>
{
    public async Task Handle(WorkflowInstanceCompletedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var instance = domainEvent.WorkflowInstance;

        if (instance.Status != WorkflowInstanceStatus.Completed)
            return;

        await mediator.Send(
            new SendNotificationCommand(
                "您的流程已审批通过",
                $"流程「{instance.Title}」已审批通过，系统将自动执行后续操作。",
                NotificationType.Workflow,
                NotificationLevel.Success,
                null,
                string.Empty,
                instance.InitiatorId.Id,
                instance.Id.ToString(),
                "WorkflowInstance"),
            cancellationToken);
    }
}
