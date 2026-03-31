using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProductCategoryAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Product;

/// <summary>
/// 获取产品分类详情（用于编辑回填）
/// </summary>
public class GetProductCategoryEndpoint(ProductCategoryQuery query)
    : EndpointWithoutRequest<ResponseData<ProductCategoryDto>>
{
    public override void Configure()
    {
        Tags("Product");
        Get("/api/admin/product-categories/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProductView);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = new ProductCategoryId(Route<Guid>("id"));
        var result = await query.GetByIdAsync(id, ct);
        if (result == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
