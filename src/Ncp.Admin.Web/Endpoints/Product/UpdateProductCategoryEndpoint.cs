using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProductCategoryAggregate;
using Ncp.Admin.Web.Application.Commands.ProductCategoryModule;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Product;

/// <summary>
/// 更新产品分类请求
/// </summary>
public record UpdateProductCategoryRequest(
    string Name,
    string Remark,
    Guid? ParentId,
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
        Put("/api/admin/product-categories/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProductEdit);
    }

    public override async Task HandleAsync(UpdateProductCategoryRequest req, CancellationToken ct)
    {
        var id = new ProductCategoryId(Route<Guid>("id"));
        var parentId = req.ParentId.HasValue && req.ParentId.Value != Guid.Empty
            ? new ProductCategoryId(req.ParentId.Value)
            : new ProductCategoryId(Guid.Empty);
        var cmd = new UpdateProductCategoryCommand(id, req.Name, req.Remark, parentId, req.SortOrder, req.Visible, req.IsDiscount);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
