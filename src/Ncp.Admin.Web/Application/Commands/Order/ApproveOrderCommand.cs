using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Order;

/// <summary>
/// 审批通过订单（工作流完成后由领域事件处理器通过 Mediator 调用）
/// </summary>
public record ApproveOrderCommand(OrderId OrderId) : ICommand;

/// <summary>
/// 审批通过订单验证器
/// </summary>
public class ApproveOrderCommandValidator : AbstractValidator<ApproveOrderCommand>
{
    public ApproveOrderCommandValidator()
    {
        RuleFor(c => c.OrderId).NotNull().WithMessage("订单ID不能为空");
    }
}

/// <summary>
/// 审批通过订单处理器：更新订单状态为已下单
/// </summary>
public class ApproveOrderCommandHandler(IOrderRepository orderRepository)
    : ICommandHandler<ApproveOrderCommand>
{
    public async Task Handle(ApproveOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetAsync(request.OrderId, cancellationToken)
            ?? throw new KnownException("未找到订单", ErrorCodes.OrderNotFound);

        order.Approve();
    }
}
