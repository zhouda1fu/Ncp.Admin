using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Domain.DomainEvents;
using Ncp.Admin.Web.Application.Services.Workflow;
using Serilog;

namespace Ncp.Admin.Web.Application.DomainEventHandlers;

/// <summary>
/// 工作流完成事件业务分发器：通知仍由独立通知处理器负责，这里只做业务回写。
/// </summary>
public class WorkflowInstanceCompletedDomainEventHandlerForBusinessDispatch(
    WorkflowBusinessAdapterDispatcher dispatcher)
    : IDomainEventHandler<WorkflowInstanceCompletedDomainEvent>
{
    public async Task Handle(WorkflowInstanceCompletedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        var instance = domainEvent.WorkflowInstance;
        if (instance.Status != WorkflowInstanceStatus.Completed)
        {
            return;
        }

        try
        {
            await dispatcher.DispatchCompletedAsync(instance, cancellationToken);
        }
        catch (Exception ex)
        {
            var failureReason = BuildFailureReason(ex);
            instance.MarkFaulted(failureReason);
            Log.Error(
                ex,
                "工作流完成业务回写失败，流程实例ID：{InstanceId}，业务类型：{BusinessType}",
                instance.Id,
                instance.BusinessType);
        }
    }

    private static string BuildFailureReason(Exception exception)
    {
        var message = exception is KnownException known
            ? known.Message
            : $"业务回写失败：{exception.Message}";
        return message.Length <= 2000 ? message : message[..2000];
    }
}

/// <summary>
/// 工作流驳回事件业务分发器。
/// </summary>
public class WorkflowInstanceRejectedDomainEventHandlerForBusinessDispatch(
    WorkflowBusinessAdapterDispatcher dispatcher)
    : IDomainEventHandler<WorkflowInstanceRejectedDomainEvent>
{
    public async Task Handle(WorkflowInstanceRejectedDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        await dispatcher.DispatchRejectedAsync(domainEvent.WorkflowInstance, cancellationToken);
    }
}

/// <summary>
/// 工作流取消事件业务分发器。
/// </summary>
public class WorkflowInstanceCancelledDomainEventHandlerForBusinessDispatch(
    WorkflowBusinessAdapterDispatcher dispatcher)
    : IDomainEventHandler<WorkflowInstanceCancelledDomainEvent>
{
    public async Task Handle(WorkflowInstanceCancelledDomainEvent domainEvent, CancellationToken cancellationToken)
    {
        await dispatcher.DispatchCancelledAsync(domainEvent.WorkflowInstance, cancellationToken);
    }
}
