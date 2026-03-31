using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Order;

/// <summary>
/// 删除订单备注（仅删除 TypeId=0：分期加急发货提示备注）
/// </summary>
public record DeleteOrderRemarkCommand(OrderId OrderId, OrderRemarkId RemarkId) : ICommand<bool>;

/// <summary>
/// 删除订单备注验证器
/// </summary>
public class DeleteOrderRemarkCommandValidator : AbstractValidator<DeleteOrderRemarkCommand>
{
    public DeleteOrderRemarkCommandValidator()
    {
        RuleFor(c => c.OrderId).NotNull();
        RuleFor(c => c.RemarkId).NotNull();
    }
}

/// <summary>
/// 删除订单备注处理器
/// </summary>
public class DeleteOrderRemarkCommandHandler(IOrderRepository orderRepository)
    : ICommandHandler<DeleteOrderRemarkCommand, bool>
{
    public async Task<bool> Handle(DeleteOrderRemarkCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetAggregateForEditAsync(request.OrderId, cancellationToken)
            ?? throw new KnownException("未找到订单", ErrorCodes.OrderNotFound);

        order.RemoveRemark(request.RemarkId, expectedTypeId: 0);
        return true;
    }
}

