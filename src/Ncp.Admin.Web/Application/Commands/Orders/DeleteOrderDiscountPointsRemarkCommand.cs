using FluentValidation;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Orders;

/// <summary>
/// 删除订单“优惠点数说明”（仅 TypeId=1）
/// </summary>
public record DeleteOrderDiscountPointsRemarkCommand(OrderId OrderId, OrderRemarkId RemarkId, UserId UserId) : ICommand<bool>;

/// <summary>
/// 删除订单“优惠点数说明”验证器
/// </summary>
public class DeleteOrderDiscountPointsRemarkCommandValidator : AbstractValidator<DeleteOrderDiscountPointsRemarkCommand>
{
    public DeleteOrderDiscountPointsRemarkCommandValidator()
    {
        RuleFor(c => c.OrderId).NotNull();
        RuleFor(c => c.RemarkId).NotNull();
        RuleFor(c => c.UserId).NotNull();
    }
}

/// <summary>
/// 删除订单“优惠点数说明”处理器
/// </summary>
public class DeleteOrderDiscountPointsRemarkCommandHandler(IOrderRepository orderRepository)
    : ICommandHandler<DeleteOrderDiscountPointsRemarkCommand, bool>
{
    public async Task<bool> Handle(DeleteOrderDiscountPointsRemarkCommand request, CancellationToken cancellationToken)
    {
        var order = await orderRepository.GetAggregateForEditAsync(request.OrderId, cancellationToken)
            ?? throw new KnownException("未找到订单", ErrorCodes.OrderNotFound);

        var remark = order.Remarks.FirstOrDefault(r => r.Id == request.RemarkId);
        if (remark == null || remark.TypeId != 1 || remark.UserId != request.UserId)
            throw new KnownException("未找到订单备注", ErrorCodes.OrderRemarkNotFound);

        order.RemoveRemark(request.RemarkId, expectedTypeId: 1);
        return true;
    }
}

