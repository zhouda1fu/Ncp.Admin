using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Order;

/// <summary>
/// 创建订单备注（固定写入 TypeId=0）
/// </summary>
public record CreateOrderRemarkCommand(OrderId OrderId, UserId UserId, string Content) : ICommand<bool>;

/// <summary>
/// 创建订单备注验证器
/// </summary>
public class CreateOrderRemarkCommandValidator : AbstractValidator<CreateOrderRemarkCommand>
{
    public CreateOrderRemarkCommandValidator()
    {
        RuleFor(c => c.OrderId).NotNull().WithMessage("订单ID不能为空");
        RuleFor(c => c.UserId).NotNull().WithMessage("用户ID不能为空");
        RuleFor(c => c.Content).NotEmpty().WithMessage("备注内容不能为空").MaximumLength(2000);
    }
}

/// <summary>
/// 创建订单备注处理器
/// </summary>
public class CreateOrderRemarkCommandHandler(IOrderRepository orderRepository)
    : ICommandHandler<CreateOrderRemarkCommand, bool>
{
    public async Task<bool> Handle(CreateOrderRemarkCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetAggregateForEditAsync(request.OrderId, cancellationToken)
            ?? throw new KnownException("未找到订单", ErrorCodes.OrderNotFound);

        // TypeId=0：分期加急发货提示备注
        order.AddRemark(request.Content, request.UserId, 0);
        return true;
    }
}

