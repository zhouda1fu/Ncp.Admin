using Ncp.Admin.Domain.AggregatesModel.NotificationAggregate;
using Ncp.Admin.Domain.DomainEvents;
using Ncp.Admin.Web.Application.Commands.Notifications;
using Ncp.Admin.Web.Application.Commands.Workflows;

namespace Ncp.Admin.Web.Application.DomainEventHandlers;

/// <summary>
/// 工作流实例取消领域事件处理器：向发起人发送取消通知。
/// </summary>
public class WorkflowInstanceCancelledDomainEventHandlerForSendNotification(IMediator mediator)
    : IDomainEventHandler<WorkflowInstanceCancelledDomainEvent>
{
    public async Task Handle(WorkflowInstanceCancelledDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var instance = domainEvent.WorkflowInstance;

        await mediator.Send(
            new SendNotificationCommand(
                "您的流程已取消",
                $"流程「{instance.Title}」已取消。",
                NotificationType.Workflow,
                NotificationLevel.Info,
                null,
                string.Empty,
                instance.InitiatorId.Id,
                instance.Id.ToString(),
                "WorkflowInstance"),
            cancellationToken);
    }
}
