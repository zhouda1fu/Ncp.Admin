using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Order;

/// <summary>
/// 驳回订单（工作流驳回时由领域事件处理器通过 Mediator 调用）
/// </summary>
public record RejectOrderCommand(OrderId OrderId) : ICommand;

/// <summary>
/// 驳回订单验证器
/// </summary>
public class RejectOrderCommandValidator : AbstractValidator<RejectOrderCommand>
{
    public RejectOrderCommandValidator()
    {
        RuleFor(c => c.OrderId).NotNull().WithMessage("订单ID不能为空");
    }
}

/// <summary>
/// 驳回订单处理器
/// </summary>
public class RejectOrderCommandHandler(IOrderRepository orderRepository)
    : ICommandHandler<RejectOrderCommand>
{
    public async Task Handle(RejectOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetAsync(request.OrderId, cancellationToken);
        if (order != null)
            order.Reject();
    }
}
