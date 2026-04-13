using MediatR;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Domain.DomainEvents;
using Ncp.Admin.Web.Application.Commands.Customers;
using Ncp.Admin.Web.Application.Commands.Workflows;
using Serilog;

namespace Ncp.Admin.Web.Application.DomainEventHandlers;

/// <summary>
/// 工作流实例完成：客户作废审批通过后执行实际作废
/// </summary>
public class WorkflowInstanceCompletedDomainEventHandlerForCustomerSeaVoid(IMediator mediator)
    : IDomainEventHandler<WorkflowInstanceCompletedDomainEvent>
{
    public async Task Handle(WorkflowInstanceCompletedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var instance = domainEvent.WorkflowInstance;

        if (instance.Status != WorkflowInstanceStatus.Completed
            || instance.BusinessType != WorkflowBusinessTypes.CustomerSeaVoid)
        {
            return;
        }

        if (!Guid.TryParse(instance.BusinessKey, out var customerGuid))
        {
            Log.Error("客户作废工作流 BusinessKey 无效: InstanceId={InstanceId}, BusinessKey={BusinessKey}",
                instance.Id, instance.BusinessKey);
            return;
        }

        try
        {
            await mediator.Send(
                new VoidCustomerCommand(new CustomerId(customerGuid), instance.InitiatorId),
                cancellationToken);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "客户作废工作流通过后执行作废失败: InstanceId={InstanceId}", instance.Id);
            await mediator.Send(
                new MarkWorkflowInstanceFaultedCommand(instance.Id, ex.Message),
                cancellationToken);
        }
    }
}
