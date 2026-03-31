using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Order;

/// <summary>
/// 回写订单工作流实例，并将订单状态置为审核中
/// </summary>
/// <param name="OrderId">订单ID</param>
/// <param name="WorkflowInstanceId">工作流实例ID</param>
public record ConfirmOrderWorkflowStartedCommand(OrderId OrderId, WorkflowInstanceId WorkflowInstanceId) : ICommand;

public class ConfirmOrderWorkflowStartedCommandValidator : AbstractValidator<ConfirmOrderWorkflowStartedCommand>
{
    public ConfirmOrderWorkflowStartedCommandValidator()
    {
        RuleFor(x => x.OrderId).NotNull().WithMessage("订单ID不能为空");
        RuleFor(x => x.WorkflowInstanceId).NotNull().WithMessage("工作流实例ID不能为空");
    }
}

public class ConfirmOrderWorkflowStartedCommandHandler(IOrderRepository orderRepository)
    : ICommandHandler<ConfirmOrderWorkflowStartedCommand>
{
    public async Task Handle(ConfirmOrderWorkflowStartedCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetAggregateForEditAsync(request.OrderId, cancellationToken)
            ?? throw new KnownException("未找到订单", ErrorCodes.OrderNotFound);

        order.ConfirmApprovalWorkflowStarted(request.WorkflowInstanceId);
    }
}
