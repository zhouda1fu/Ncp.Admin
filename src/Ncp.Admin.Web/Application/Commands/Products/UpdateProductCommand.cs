using FluentValidation;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProductCategoryAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProductTypeAggregate;
using Ncp.Admin.Domain.AggregatesModel.SupplierAggregate;
using Ncp.Admin.Infrastructure.Repositories;
using Ncp.Admin.Web.Utils;

namespace Ncp.Admin.Web.Application.Commands.Products;

/// <summary>
/// 更新产品命令（所有字段必填；分类与供应商可选）
/// </summary>
/// <param name="Id">产品 ID</param>
/// <param name="ProductTypeId">产品类型 ID</param>
/// <param name="Status">状态（是否有效）</param>
/// <param name="Name">产品名称</param>
/// <param name="Code">产品编号/货号</param>
/// <param name="Model">型号</param>
/// <param name="Unit">单位</param>
/// <param name="Barcode">条码（可空字符串）</param>
/// <param name="ActivationCode">激活码</param>
/// <param name="PriceStandard">价格标准</param>
/// <param name="MarketSales">市场销售</param>
/// <param name="Description">描述（可空字符串）</param>
/// <param name="CostPrice">成本价（≥0）</param>
/// <param name="CustomerPrice">客户价（≥0）</param>
/// <param name="Qty">库存数量（≥0）</param>
/// <param name="Tags">标签（可空字符串）</param>
/// <param name="Feature">功能特点（可空字符串）</param>
/// <param name="Configuration">硬件配置（可空字符串）</param>
/// <param name="Instructions">使用说明（可空字符串）</param>
/// <param name="InstallProcess">操作流程（可空字符串）</param>
/// <param name="OperationProcessResources">操作流程资源 JSON</param>
/// <param name="Introduction">产品介绍（可空字符串）</param>
/// <param name="IntroductionResources">产品介绍资源 JSON</param>
/// <param name="ImagePath">图片路径（可空字符串）</param>
/// <param name="CategoryId">产品分类 ID（无则由端点传 new ProductCategoryId(Guid.Empty)）</param>
/// <param name="SupplierId">供应商 ID（无则由端点传 new SupplierId(Guid.Empty)）</param>
public record UpdateProductCommand(
    ProductId Id,
    ProductTypeId ProductTypeId,
    bool Status,
    string Name,
    string Code,
    string Model,
    string Unit,
    string Barcode,
    string ActivationCode,
    string PriceStandard,
    string MarketSales,
    string Description,
    decimal CostPrice,
    decimal CustomerPrice,
    int Qty,
    string Tags,
    string Feature,
    string Configuration,
    string Instructions,
    string InstallProcess,
    string OperationProcessResources,
    string Introduction,
    string IntroductionResources,
    string ImagePath,
    ProductCategoryId CategoryId,
    SupplierId SupplierId)
    : ICommand<bool>;

public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(c => c.Id).NotEmpty();
        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Code).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Model).NotNull().MaximumLength(100);
        RuleFor(c => c.Unit).NotEmpty().MaximumLength(20);
        RuleFor(c => c.Barcode).NotNull().MaximumLength(50);
        RuleFor(c => c.Description).NotNull().MaximumLength(4000);
        RuleFor(c => c.CostPrice).GreaterThanOrEqualTo(0);
        RuleFor(c => c.CustomerPrice).GreaterThanOrEqualTo(0);
        RuleFor(c => c.Qty).GreaterThanOrEqualTo(0);
        RuleFor(c => c.Tags).NotNull().MaximumLength(200);
        RuleFor(c => c.Feature).NotNull().MaximumLength(4000);
        RuleFor(c => c.Configuration).NotNull().MaximumLength(4000);
        RuleFor(c => c.Instructions).NotNull().MaximumLength(4000);
        RuleFor(c => c.InstallProcess).NotNull().MaximumLength(4000);
        RuleFor(c => c.Introduction).NotNull().MaximumLength(4000);
        RuleFor(c => c.ImagePath).NotNull().MaximumLength(500);
    }
}

public class UpdateProductCommandHandler(IProductRepository repository)
    : ICommandHandler<UpdateProductCommand, bool>
{
    public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await repository.GetAsync(request.Id, cancellationToken)
            ?? throw new KnownException("未找到产品", ErrorCodes.ProductNotFound);
        product.Update(
            request.ProductTypeId,
            request.Status,
            request.Name,
            request.Code,
            request.Model,
            request.Unit,
            request.Barcode,
            request.ActivationCode ?? string.Empty,
            request.PriceStandard ?? string.Empty,
            request.MarketSales ?? string.Empty,
            request.Description,
            request.CostPrice,
            request.CustomerPrice,
            request.Qty,
            request.Tags,
            request.Feature,
            request.Configuration,
            request.Instructions,
            request.InstallProcess,
            request.OperationProcessResources ?? string.Empty,
            request.Introduction,
            request.IntroductionResources ?? string.Empty,
            request.ImagePath,
            request.CategoryId,
            request.SupplierId);
        return true;
    }
}
