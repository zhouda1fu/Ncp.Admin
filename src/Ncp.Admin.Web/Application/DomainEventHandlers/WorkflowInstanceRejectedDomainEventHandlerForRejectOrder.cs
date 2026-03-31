using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Domain.DomainEvents.WorkflowEvents;
using Ncp.Admin.Web.Application.Commands.Order;
using Ncp.Admin.Web.Application.Commands.Workflow;

namespace Ncp.Admin.Web.Application.DomainEventHandlers;

/// <summary>
/// 工作流实例驳回领域事件处理器：订单被驳回时更新订单状态
/// </summary>
public class WorkflowInstanceRejectedDomainEventHandlerForRejectOrder(IMediator mediator)
    : IDomainEventHandler<WorkflowInstanceRejectedDomainEvent>
{
    public async Task Handle(WorkflowInstanceRejectedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var instance = domainEvent.WorkflowInstance;

        if (instance.BusinessType != WorkflowBusinessTypes.Order || !Guid.TryParse(instance.BusinessKey, out var orderIdGuid))
            return;

        await mediator.Send(new RejectOrderCommand(new OrderId(orderIdGuid)), cancellationToken);
    }
}
