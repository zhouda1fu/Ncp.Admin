using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProductCategoryAggregate;
using Ncp.Admin.Web.Application.Commands.ProductCategories;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Products;

/// <summary>
/// 更新产品分类请求
/// </summary>
public record UpdateProductCategoryRequest(
    string Name,
    string Remark,
    ProductCategoryId? ParentId,
    int SortOrder,
    bool Visible,
    bool IsDiscount);

/// <summary>
/// 更新产品分类
/// </summary>
public class UpdateProductCategoryEndpoint(IMediator mediator)
    : Endpoint<UpdateProductCategoryRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Product");
        Description(b => b.AutoTagOverride("Product").WithSummary("更新产品分类"));
        Put("/api/admin/product-categories/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProductEdit);
    }

    public override async Task HandleAsync(UpdateProductCategoryRequest req, CancellationToken ct)
    {
        var id = new ProductCategoryId(Route<Guid>("id"));
        var rootId = ProductCategory.RootParentId;
        var parentId = req.ParentId is { } p && p != rootId ? p : rootId;
        var cmd = new UpdateProductCategoryCommand(id, req.Name, req.Remark, parentId, req.SortOrder, req.Visible, req.IsDiscount);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
