using Ncp.Admin.Domain.DomainEvents;
using Ncp.Admin.Web.Application.Commands.Orders;

namespace Ncp.Admin.Web.Application.DomainEventHandlers;

/// <summary>
/// 订单内容快照已更新后：若处于审核中且关联订单审批流，则同步工作流变量
/// </summary>
public class OrderUpdatedDomainEventHandlerForSyncWorkflowVariables(IMediator mediator)
    : IDomainEventHandler<OrderUpdatedDomainEvent>
{
    public async Task Handle(OrderUpdatedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        await mediator.Send(new SyncOrderWorkflowVariablesCommand(domainEvent.Order.Id), cancellationToken);
    }
}
