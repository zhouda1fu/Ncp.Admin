using Ncp.Admin.Domain.AggregatesModel.LeaveRequestAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Domain.DomainEvents.WorkflowEvents;
using Ncp.Admin.Web.Application.Commands.Leave;
using Ncp.Admin.Web.Application.Commands.Workflow;
using Serilog;

namespace Ncp.Admin.Web.Application.DomainEventHandlers;

/// <summary>
/// 工作流实例完成领域事件处理器：请假申请审批通过时更新请假单状态并扣减余额
/// </summary>
public class WorkflowInstanceCompletedDomainEventHandlerForApproveLeaveRequest(IMediator mediator)
    : IDomainEventHandler<WorkflowInstanceCompletedDomainEvent>
{
    public async Task Handle(WorkflowInstanceCompletedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var instance = domainEvent.WorkflowInstance;

        if (instance.Status != WorkflowInstanceStatus.Completed || instance.BusinessType != WorkflowBusinessTypes.LeaveRequest)
            return;

        if (!Guid.TryParse(instance.BusinessKey, out var leaveIdGuid))
        {
            Log.Error("请假申请ID无效: BusinessKey={BusinessKey}", instance.BusinessKey);
            return;
        }

        await mediator.Send(new ApproveLeaveRequestCommand(new LeaveRequestId(leaveIdGuid)), cancellationToken);
    }
}
