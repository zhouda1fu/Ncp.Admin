using Ncp.Admin.Domain.AggregatesModel.LeaveRequestAggregate;
using Ncp.Admin.Domain.DomainEvents;
using Ncp.Admin.Web.Application.Commands.Leaves;
using Ncp.Admin.Web.Application.Commands.Workflows;

namespace Ncp.Admin.Web.Application.DomainEventHandlers;

/// <summary>
/// 工作流实例驳回领域事件处理器：请假申请被驳回时更新请假单状态
/// </summary>
public class WorkflowInstanceRejectedDomainEventHandlerForRejectLeaveRequest(IMediator mediator)
    : IDomainEventHandler<WorkflowInstanceRejectedDomainEvent>
{
    public async Task Handle(WorkflowInstanceRejectedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var instance = domainEvent.WorkflowInstance;

        if (instance.BusinessType != WorkflowBusinessTypes.LeaveRequest || !Guid.TryParse(instance.BusinessKey, out var leaveIdGuid))
            return;

        await mediator.Send(new RejectLeaveRequestCommand(new LeaveRequestId(leaveIdGuid)), cancellationToken);
    }
}
