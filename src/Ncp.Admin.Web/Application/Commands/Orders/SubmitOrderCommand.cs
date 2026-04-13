using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Orders;

/// <summary>
/// 提交订单审批（草稿或已驳回的订单可发起或重新发起审批流程）
/// </summary>
/// <param name="OrderId">订单 ID</param>
/// <param name="Remark">备注</param>
public record SubmitOrderCommand(OrderId OrderId, string Remark = "") : ICommand;

public class SubmitOrderCommandValidator : AbstractValidator<SubmitOrderCommand>
{
    public SubmitOrderCommandValidator()
    {
        RuleFor(c => c.OrderId).NotNull().WithMessage("订单ID不能为空");
    }
}

public class SubmitOrderCommandHandler(
    IOrderRepository orderRepository) : ICommandHandler<SubmitOrderCommand>
{
    public async Task Handle(SubmitOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetAggregateForEditAsync(request.OrderId, cancellationToken)
            ?? throw new KnownException("未找到订单", ErrorCodes.OrderNotFound);
        order.RequestOrderApprovalWorkflowStart(request.Remark ?? string.Empty);
    }
}
