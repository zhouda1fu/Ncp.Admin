using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProductCategoryAggregate;
using Ncp.Admin.Web.Application.Commands.ProductCategories;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Products;

/// <summary>
/// 删除产品分类
/// </summary>
public class DeleteProductCategoryEndpoint(IMediator mediator)
    : EndpointWithoutRequest<ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Product");
        Description(b => b.AutoTagOverride("Product").WithSummary("删除产品分类"));
        Delete("/api/admin/product-categories/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProductEdit);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = new ProductCategoryId(Route<Guid>("id"));
        await mediator.Send(new DeleteProductCategoryCommand(id), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
