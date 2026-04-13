using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Orders;

/// <summary>
/// 创建订单“优惠点数说明”（固定写入 TypeId=1）
/// </summary>
public record CreateOrderDiscountPointsRemarkCommand(OrderId OrderId, UserId UserId, string Content) : ICommand<bool>;

/// <summary>
/// 创建订单“优惠点数说明”验证器
/// </summary>
public class CreateOrderDiscountPointsRemarkCommandValidator : AbstractValidator<CreateOrderDiscountPointsRemarkCommand>
{
    public CreateOrderDiscountPointsRemarkCommandValidator()
    {
        RuleFor(c => c.OrderId).NotNull().WithMessage("订单ID不能为空");
        RuleFor(c => c.UserId).NotNull().WithMessage("用户ID不能为空");
        RuleFor(c => c.Content).NotEmpty().WithMessage("优惠点数说明内容不能为空").MaximumLength(2000);
    }
}

/// <summary>
/// 创建订单“优惠点数说明”处理器
/// </summary>
public class CreateOrderDiscountPointsRemarkCommandHandler(IOrderRepository orderRepository)
    : ICommandHandler<CreateOrderDiscountPointsRemarkCommand, bool>
{
    public async Task<bool> Handle(CreateOrderDiscountPointsRemarkCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetAggregateForEditAsync(request.OrderId, cancellationToken)
            ?? throw new KnownException("未找到订单", ErrorCodes.OrderNotFound);

        order.AddRemark(request.Content, request.UserId, typeId: 1);
        return true;
    }
}

