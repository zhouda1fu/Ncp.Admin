using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Application.Commands.Workflow;
using Ncp.Admin.Web.Application.Services.Workflow;

namespace Ncp.Admin.Web.Application.Commands.Order;

/// <summary>
/// 若订单处于审核中且已关联运行中的订单审批工作流，则将当前订单快照同步为工作流变量
/// </summary>
/// <param name="OrderId">订单ID</param>
public record SyncOrderWorkflowVariablesCommand(OrderId OrderId) : ICommand;

public class SyncOrderWorkflowVariablesCommandValidator : AbstractValidator<SyncOrderWorkflowVariablesCommand>
{
    public SyncOrderWorkflowVariablesCommandValidator()
    {
        RuleFor(c => c.OrderId).NotNull().WithMessage("订单ID不能为空");
    }
}

public class SyncOrderWorkflowVariablesCommandHandler(
    IOrderRepository orderRepository,
    IWorkflowInstanceRepository workflowInstanceRepository) : ICommandHandler<SyncOrderWorkflowVariablesCommand>
{
    public async Task Handle(SyncOrderWorkflowVariablesCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetAggregateForEditAsync(request.OrderId, cancellationToken)
            ?? throw new KnownException("未找到订单", ErrorCodes.OrderNotFound);

        if (order.WorkflowInstanceId == new WorkflowInstanceId(Guid.Empty) || order.Status != OrderStatus.PendingAudit)
            return;

        var instance = await workflowInstanceRepository.GetAsync(order.WorkflowInstanceId, cancellationToken);
        if (instance is null
            || instance.Status != WorkflowInstanceStatus.Running
            || !string.Equals(instance.BusinessType, WorkflowBusinessTypes.Order, StringComparison.Ordinal)
            || !string.Equals(instance.BusinessKey, order.Id.ToString(), StringComparison.OrdinalIgnoreCase))
            return;

        instance.UpdateVariables(OrderWorkflowVariablesBuilder.BuildJson(order));
    }
}
