using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProductCategoryAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProductTypeAggregate;
using Ncp.Admin.Domain.AggregatesModel.SupplierAggregate;
using Ncp.Admin.Web.Application.Commands.Product;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Product;

/// <summary>
/// 更新产品请求（Id 来自路由）
/// </summary>
/// <param name="ProductTypeId">产品类型 ID</param>
/// <param name="Status">状态（是否有效）</param>
/// <param name="Name">产品名称（必填）</param>
/// <param name="Code">产品编号/货号（必填）</param>
/// <param name="Model">型号</param>
/// <param name="Unit">单位（必填）</param>
/// <param name="Barcode">条码</param>
/// <param name="ActivationCode">激活码</param>
/// <param name="PriceStandard">价格标准</param>
/// <param name="MarketSales">市场销售</param>
/// <param name="Description">描述</param>
/// <param name="CostPrice">成本价（≥0）</param>
/// <param name="CustomerPrice">客户价（≥0）</param>
/// <param name="Qty">库存数量（≥0）</param>
/// <param name="Tags">标签</param>
/// <param name="Feature">功能特点</param>
/// <param name="Configuration">硬件配置</param>
/// <param name="Instructions">使用说明</param>
/// <param name="InstallProcess">操作流程</param>
/// <param name="OperationProcessResources">操作流程资源 JSON</param>
/// <param name="Introduction">产品介绍</param>
/// <param name="IntroductionResources">产品介绍资源 JSON</param>
/// <param name="ImagePath">图片路径</param>
/// <param name="CategoryId">产品分类 ID（可选）</param>
/// <param name="SupplierId">供应商 ID（可选）</param>
public record UpdateProductRequest(
    Guid ProductTypeId,
    bool Status,
    string Name,
    string Code,
    string Model,
    string Unit,
    string Barcode,
    string? ActivationCode,
    string? PriceStandard,
    string? MarketSales,
    string Description,
    decimal CostPrice,
    decimal CustomerPrice,
    int Qty,
    string Tags,
    string Feature,
    string Configuration,
    string Instructions,
    string InstallProcess,
    string? OperationProcessResources,
    string Introduction,
    string? IntroductionResources,
    string ImagePath,
    Guid? CategoryId = null,
    Guid? SupplierId = null);

/// <summary>
/// 更新产品
/// </summary>
public class UpdateProductEndpoint(IMediator mediator)
    : Endpoint<UpdateProductRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Product");
        Put("/api/admin/products/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProductEdit);
    }

    public override async Task HandleAsync(UpdateProductRequest req, CancellationToken ct)
    {
        var id = new ProductId(Route<Guid>("id"));
        var productTypeId = new ProductTypeId(req.ProductTypeId);
        var categoryId = req.CategoryId.HasValue && req.CategoryId.Value != Guid.Empty ? new ProductCategoryId(req.CategoryId.Value) : new ProductCategoryId(Guid.Empty);
        var supplierId = req.SupplierId.HasValue && req.SupplierId.Value != Guid.Empty ? new SupplierId(req.SupplierId.Value) : new SupplierId(Guid.Empty);
        var cmd = new UpdateProductCommand(
            id,
            productTypeId,
            req.Status,
            req.Name ?? "",
            req.Code ?? "",
            req.Model ?? "",
            req.Unit ?? "",
            req.Barcode ?? "",
            req.ActivationCode ?? "",
            req.PriceStandard ?? "",
            req.MarketSales ?? "",
            req.Description ?? "",
            req.CostPrice,
            req.CustomerPrice,
            req.Qty,
            req.Tags ?? "",
            req.Feature ?? "",
            req.Configuration ?? "",
            req.Instructions ?? "",
            req.InstallProcess ?? "",
            req.OperationProcessResources ?? "",
            req.Introduction ?? "",
            req.IntroductionResources ?? "",
            req.ImagePath ?? "",
            categoryId,
            supplierId);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
