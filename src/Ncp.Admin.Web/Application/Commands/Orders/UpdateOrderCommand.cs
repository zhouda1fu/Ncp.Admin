using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProductCategoryAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProductTypeAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.OrderInvoiceTypeOptionAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Repositories;

namespace Ncp.Admin.Web.Application.Commands.Orders;

/// <summary>
/// 更新订单时传入的明细行 DTO（所有字段必填）
/// </summary>
public record UpdateOrderItemDto(
    ProductId ProductId,
    ProductCategoryId ProductCategoryId,
    ProductTypeId ProductTypeId,
    string ImagePath,
    string InstallNotes,
    string TrainingDuration,
    int PackingStatus,
    int ReviewStatus,
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
    string customerName,
    ProjectId projectId,
    string orderNumber,
    OrderType type,
    decimal amount,
    string remark,
    UserId ownerId,
    string ownerName,
    DeptId deptId,
    string deptName,
    string projectContactName,
    string projectContactPhone,
    string warranty,
    string contractSigningCompany,
    string contractTrustee,
    bool needInvoice,
    OrderInvoiceTypeOptionId invoiceTypeId,
    decimal installationFee,
    decimal estimatedFreight,
    string contractFilesJson,
    string stockFilesJson,
    SelectedContractFileId selectedContractFileId,
    bool isShipped,
    PaymentStatus paymentStatus,
    bool contractNotCompanyTemplate,
    decimal contractAmount,
    string receiverName,
    string receiverPhone,
    string receiverAddress,
    DateTimeOffset payDate,
    DateTimeOffset deliveryDate,
    OrderLogisticsCompanyId orderLogisticsCompanyId,
    OrderLogisticsMethodId orderLogisticsMethodId,
    LogisticsPaymentMethodId logisticsPaymentMethodId,
    string waybillNumber,
    decimal shippingFee,
    bool shippingFeeIsPay,
    decimal surcharge,
    bool isNoLogo,
    string afterSalesServiceId,
    bool isAssess,
    string comments,
    DateTimeOffset startDate,
    DateTimeOffset endDate,
    bool isRed,
    bool isFree,
    bool isRepay,
    DateTimeOffset repayDate,
    DateTimeOffset fRepayDate,
    DateTimeOffset delayDate,
    string delayReason,
    string feedback,
    string scontent,
    UserId warehousePickerId,
    UserId warehouseTechId,
    UserId warehouseReviewerId,
    WarehouseStatus warehouseStatus,
    IReadOnlyList<OrderCategoryContractItem> OrderCategories,
    IReadOnlyList<UpdateOrderItemDto> Items) : ICommand<bool>;

public class UpdateOrderCommandValidator : AbstractValidator<UpdateOrderCommand>
{
    public UpdateOrderCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.customerName).NotEmpty().MaximumLength(200);
        RuleFor(c => c.projectId).NotEmpty();
        RuleFor(c => c.orderNumber).NotEmpty().MaximumLength(100);
        RuleFor(c => c.ownerId).NotEmpty();
        RuleFor(c => c.deptId).NotEmpty();
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
        RuleForEach(c => c.OrderCategories).ChildRules(b =>
        {
            b.RuleFor(x => x.ProductCategoryId).NotEmpty();
            b.RuleFor(x => x.CategoryName).NotEmpty().MaximumLength(100);
            b.RuleFor(x => x.DiscountPoints).GreaterThanOrEqualTo(0);
            b.RuleFor(x => x.Remark).MaximumLength(500);
        });
        RuleFor(c => c).Custom((cmd, ctx) =>
        {
            if (cmd.OrderCategories.Count == 0)
                return;
            var empty = new ProductCategoryId(Guid.Empty);
            var itemCategorySet = cmd.Items
                .Select(i => i.ProductCategoryId)
                .Where(id => id != empty)
                .ToHashSet();
            foreach (var b in cmd.OrderCategories)
            {
                if (b.ProductCategoryId == empty)
                {
                    ctx.AddFailure(nameof(cmd.OrderCategories), "按分类合同优惠的产品分类不能为空");
                    return;
                }

                if (!itemCategorySet.Contains(b.ProductCategoryId))
                    ctx.AddFailure(nameof(cmd.OrderCategories), "按分类合同优惠必须对应订单明细中已存在的产品分类");
            }
        });
    }
}

public class UpdateOrderCommandHandler(IOrderRepository repository) : ICommandHandler<UpdateOrderCommand, bool>
{
    public async Task<bool> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await repository.GetAggregateForEditAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到订单", ErrorCodes.OrderNotFound);
        if (order.IsDeleted)
            throw new KnownException("订单已删除", ErrorCodes.OrderNotFound);
        var categoryLines = request.OrderCategories
            .Select(x => (x.ProductCategoryId, x.CategoryName ?? string.Empty, x.DiscountPoints, x.Remark ?? string.Empty))
            .ToList();
        var items = request.Items
            .Select(x => new OrderItemData(
                x.ProductId,
                x.ProductCategoryId,
                x.ProductTypeId,
                x.ImagePath ?? string.Empty,
                x.InstallNotes ?? string.Empty,
                x.TrainingDuration ?? string.Empty,
                x.PackingStatus,
                x.ReviewStatus,
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
            request.customerName ?? string.Empty,
            request.projectId,
            request.orderNumber,
            request.type,
            request.amount,
            request.remark ?? string.Empty,
            request.ownerId,
            request.ownerName ?? string.Empty,
            request.deptId,
            request.deptName ?? string.Empty,
            request.projectContactName ?? string.Empty,
            request.projectContactPhone ?? string.Empty,
            request.warranty ?? string.Empty,
            request.contractSigningCompany ?? string.Empty,
            request.contractTrustee ?? string.Empty,
            request.needInvoice,
            request.invoiceTypeId,
            request.installationFee,
            request.estimatedFreight,
            request.contractFilesJson ?? "[]",
            request.stockFilesJson ?? "[]",
            request.selectedContractFileId,
            request.isShipped,
            request.paymentStatus,
            request.contractNotCompanyTemplate,
            request.contractAmount,
            request.receiverName ?? string.Empty,
            request.receiverPhone ?? string.Empty,
            request.receiverAddress ?? string.Empty,
            request.payDate,
            request.deliveryDate,
            request.orderLogisticsCompanyId,
            request.orderLogisticsMethodId,
            request.logisticsPaymentMethodId,
            request.waybillNumber ?? string.Empty,
            request.shippingFee,
            request.shippingFeeIsPay,
            request.surcharge,
            request.isNoLogo,
            request.afterSalesServiceId,
            request.isAssess,
            request.comments,
            request.startDate,
            request.endDate,
            request.isRed,
            request.isFree,
            request.isRepay,
            request.repayDate,
            request.fRepayDate,
            request.delayDate,
            request.delayReason,
            request.feedback,
            request.scontent,
            request.warehousePickerId,
            request.warehouseTechId,
            request.warehouseReviewerId,
            request.warehouseStatus,
            items);
        order.SyncOrderCategories(categoryLines);
        return true;
    }
}
