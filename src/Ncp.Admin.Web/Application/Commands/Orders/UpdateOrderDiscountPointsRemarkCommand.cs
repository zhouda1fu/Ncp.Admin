using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Orders;

/// <summary>
/// 更新订单“优惠点数说明”（仅 TypeId=1）
/// </summary>
public record UpdateOrderDiscountPointsRemarkCommand(OrderId OrderId, OrderRemarkId RemarkId, UserId UserId, string Content)
    : ICommand<bool>;

/// <summary>
/// 更新订单“优惠点数说明”验证器
/// </summary>
public class UpdateOrderDiscountPointsRemarkCommandValidator : AbstractValidator<UpdateOrderDiscountPointsRemarkCommand>
{
    public UpdateOrderDiscountPointsRemarkCommandValidator()
    {
        RuleFor(c => c.OrderId).NotNull().WithMessage("订单ID不能为空");
        RuleFor(c => c.RemarkId).NotNull().WithMessage("备注ID不能为空");
        RuleFor(c => c.Content).NotEmpty().WithMessage("优惠点数说明内容不能为空").MaximumLength(2000);
    }
}

/// <summary>
/// 更新订单“优惠点数说明”处理器
/// </summary>
public class UpdateOrderDiscountPointsRemarkCommandHandler(IOrderRepository orderRepository)
    : ICommandHandler<UpdateOrderDiscountPointsRemarkCommand, bool>
{
    public async Task<bool> Handle(UpdateOrderDiscountPointsRemarkCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetAggregateForEditAsync(request.OrderId, cancellationToken)
            ?? throw new KnownException("未找到订单", ErrorCodes.OrderNotFound);

        var remark = order.Remarks.FirstOrDefault(r => r.Id == request.RemarkId);
        if (remark == null || remark.TypeId != 1 || remark.UserId != request.UserId)
            throw new KnownException("未找到订单备注", ErrorCodes.OrderRemarkNotFound);

        order.ChangeRemarkContent(request.RemarkId, expectedTypeId: 1, request.Content);
        return true;
    }
}

