using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Orders;

/// <summary>
/// 创建订单时传入的明细行 DTO（所有字段必填）
/// </summary>
public record CreateOrderItemDto(
    ProductId ProductId,
    string ProductName,
    string Model,
    string Number,
    int Qty,
    string Unit,
    decimal Price,
    decimal Amount,
    string Remark);

public record CreateOrderCommand(
    CustomerId CustomerId,
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
    DeptId DeptId,
    string DeptName,
    string ProjectContactName,
    string ProjectContactPhone,
    string Warranty,
    string ContractSigningCompany,
    string ContractTrustee,
    bool NeedInvoice,
    decimal InstallationFee,
    decimal EstimatedFreight,
    string ContractFilesJson,
    string SelectedContractFileId,
    bool IsShipped,
    string PaymentStatus,
    bool ContractNotCompanyTemplate,
    decimal ContractDiscount,
    decimal ContractAmount,
    string ReceiverName,
    string ReceiverPhone,
    string ReceiverAddress,
    DateTimeOffset PayDate,
    DateTimeOffset DeliveryDate,
    UserId CreatorId,
    IReadOnlyList<CreateOrderItemDto> Items) : ICommand<OrderId>;

public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(c => c.CustomerId).NotEmpty();
        RuleFor(c => c.CustomerName).NotEmpty().MaximumLength(200);
        RuleFor(c => c.ProjectId).NotEmpty();
        RuleFor(c => c.ContractId).NotEmpty();
        RuleFor(c => c.OrderNumber).NotEmpty().MaximumLength(100);
        RuleFor(c => c.OwnerId).NotEmpty();
        RuleFor(c => c.DeptId).NotEmpty();
        RuleFor(c => c.CreatorId).NotEmpty();
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

public class CreateOrderCommandHandler(IOrderRepository repository) : ICommandHandler<CreateOrderCommand, OrderId>
{
    public async Task<OrderId> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
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
        var order = Order.Create(
            request.CustomerId,
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
            request.DeptId,
            request.DeptName ?? string.Empty,
            request.ProjectContactName ?? string.Empty,
            request.ProjectContactPhone ?? string.Empty,
            request.Warranty ?? string.Empty,
            request.ContractSigningCompany ?? string.Empty,
            request.ContractTrustee ?? string.Empty,
            request.NeedInvoice,
            request.InstallationFee,
            request.EstimatedFreight,
            request.ContractFilesJson ?? "[]",
            request.SelectedContractFileId ?? string.Empty,
            request.IsShipped,
            request.PaymentStatus ?? string.Empty,
            request.ContractNotCompanyTemplate,
            request.ContractDiscount,
            request.ContractAmount,
            request.ReceiverName ?? string.Empty,
            request.ReceiverPhone ?? string.Empty,
            request.ReceiverAddress ?? string.Empty,
            request.PayDate,
            request.DeliveryDate,
            request.CreatorId,
            items);
        await repository.AddAsync(order, cancellationToken);
        return order.Id;
    }
}
