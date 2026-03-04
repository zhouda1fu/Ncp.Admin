using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Orders;

/// <summary>
/// 更新订单时传入的明细行 DTO（所有字段必填）
/// </summary>
public record UpdateOrderItemDto(
    ProductId ProductId,
    string ProductName,
    string Model,
    string Number,
    int Qty,
    string Unit,
    decimal Price,
    decimal Amount,
    string Remark);

public record UpdateOrderCommand(
    OrderId Id,
    string CustomerName,
    ProjectId ProjectId,
    ContractId ContractId,
    string OrderNumber,
    OrderType Type,
    OrderStatus Status,
    decimal Amount,
    string Remark,
    UserId OwnerId,
    string OwnerName,
    string ReceiverName,
    string ReceiverPhone,
    string ReceiverAddress,
    DateTimeOffset PayDate,
    DateTimeOffset DeliveryDate,
    IReadOnlyList<UpdateOrderItemDto> Items) : ICommand<bool>;

public class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
{
    public UpdateOrderCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.CustomerName).NotEmpty().MaximumLength(200);
        RuleFor(c => c.ProjectId).NotEmpty();
        RuleFor(c => c.ContractId).NotEmpty();
        RuleFor(c => c.OrderNumber).NotEmpty().MaximumLength(100);
        RuleFor(c => c.OwnerId).NotEmpty();
        RuleFor(c => c.Items).NotEmpty();
        RuleForEach(c => c.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId).NotEmpty();
            item.RuleFor(x => x.ProductName).NotEmpty().MaximumLength(200);
            item.RuleFor(x => x.Number).NotEmpty().MaximumLength(50);
            item.RuleFor(x => x.Qty).GreaterThan(0);
            item.RuleFor(x => x.Unit).NotEmpty().MaximumLength(20);
            item.RuleFor(x => x.Remark).NotNull().MaximumLength(500);
        });
    }
}

public class UpdateOrderCommandHandler(IOrderRepository repository) : ICommandHandler<UpdateOrderCommand, bool>
{
    public async Task<bool> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到订单", ErrorCodes.OrderNotFound);
        if (order.IsDeleted)
            throw new KnownException("订单已删除", ErrorCodes.OrderNotFound);
        var items = request.Items
            .Select(x => new OrderItemData(
                x.ProductId,
                x.ProductName ?? string.Empty,
                x.Model ?? string.Empty,
                x.Number ?? string.Empty,
                x.Qty,
                x.Unit ?? string.Empty,
                x.Price,
                x.Amount,
                x.Remark ?? string.Empty))
            .ToList();
        order.Update(
            request.CustomerName ?? string.Empty,
            request.ProjectId,
            request.ContractId,
            request.OrderNumber,
            request.Type,
            request.Status,
            request.Amount,
            request.Remark ?? string.Empty,
            request.OwnerId,
            request.OwnerName ?? string.Empty,
            request.ReceiverName ?? string.Empty,
            request.ReceiverPhone ?? string.Empty,
            request.ReceiverAddress ?? string.Empty,
            request.PayDate,
            request.DeliveryDate,
            items);
        return true;
    }
}
