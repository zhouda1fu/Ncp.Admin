using Ncp.Admin.Domain.AggregatesModel.NotificationAggregate;
using Ncp.Admin.Domain.DomainEvents.WorkflowEvents;
using Ncp.Admin.Web.Application.Commands.Notification;

namespace Ncp.Admin.Web.Application.DomainEventHandlers;

/// <summary>
/// 工作流实例驳回领域事件处理器：向发起人发送驳回通知
/// </summary>
public class WorkflowInstanceRejectedDomainEventHandler(IMediator mediator) : IDomainEventHandler<WorkflowInstanceRejectedDomainEvent>
{
    public async Task Handle(WorkflowInstanceRejectedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var instance = domainEvent.WorkflowInstance;

        var title = "您的流程已被驳回";
        var content = $"流程「{instance.Title}」已被驳回，请查看详情。";

        await mediator.Send(
            new SendNotificationCommand(
                title,
                content,
                NotificationType.Workflow,
                NotificationLevel.Warning,
                null,
                string.Empty,
                instance.InitiatorId.Id,
                instance.Id.ToString(),
                "WorkflowInstance"),
            cancellationToken);
    }
}
