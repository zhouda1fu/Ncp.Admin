using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Domain.DomainEvents;
using Ncp.Admin.Web.Application.Commands.Orders;
using Ncp.Admin.Web.Application.Commands.Workflows;
using Serilog;

namespace Ncp.Admin.Web.Application.DomainEventHandlers;

/// <summary>
/// 工作流实例完成领域事件处理器：订单审批通过时更新订单状态为已下单
/// </summary>
public class WorkflowInstanceCompletedDomainEventHandlerForApproveOrder(IMediator mediator)
    : IDomainEventHandler<WorkflowInstanceCompletedDomainEvent>
{
    public async Task Handle(WorkflowInstanceCompletedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var instance = domainEvent.WorkflowInstance;

        if (instance.Status != WorkflowInstanceStatus.Completed || instance.BusinessType != WorkflowBusinessTypes.Order)
            return;

        if (!Guid.TryParse(instance.BusinessKey, out var orderIdGuid))
        {
            Log.Error("订单ID无效: BusinessKey={BusinessKey}", instance.BusinessKey);
            return;
        }

        await mediator.Send(new ApproveOrderCommand(new OrderId(orderIdGuid)), cancellationToken);
    }
}
