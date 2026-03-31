using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Order;

/// <summary>
/// 更新订单备注内容（仅 TypeId=0：分期加急发货提示备注）
/// </summary>
public record UpdateOrderRemarkCommand(OrderId OrderId, OrderRemarkId RemarkId, string Content) : ICommand<bool>;

/// <summary>
/// 更新订单备注验证器
/// </summary>
public class UpdateOrderRemarkCommandValidator : AbstractValidator<UpdateOrderRemarkCommand>
{
    public UpdateOrderRemarkCommandValidator()
    {
        RuleFor(c => c.OrderId).NotNull();
        RuleFor(c => c.RemarkId).NotNull();
        RuleFor(c => c.Content).NotEmpty().MaximumLength(2000);
    }
}

/// <summary>
/// 更新订单备注处理器
/// </summary>
public class UpdateOrderRemarkCommandHandler(IOrderRepository orderRepository) : ICommandHandler<UpdateOrderRemarkCommand, bool>
{
    public async Task<bool> Handle(UpdateOrderRemarkCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetAggregateForEditAsync(request.OrderId, cancellationToken)
            ?? throw new KnownException("未找到订单", ErrorCodes.OrderNotFound);

        order.ChangeRemarkContent(request.RemarkId, expectedTypeId: 0, request.Content);
        return true;
    }
}

