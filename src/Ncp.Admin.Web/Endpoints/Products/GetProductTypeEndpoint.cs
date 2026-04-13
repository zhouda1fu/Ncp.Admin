using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProductTypeAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Products;

/// <summary>
/// 获取产品类型详情（用于编辑回填）
/// </summary>
public class GetProductTypeEndpoint(ProductTypeQuery query)
    : EndpointWithoutRequest<ResponseData<ProductTypeDto>>
{
    public override void Configure()
    {
        Tags("Product");
        Description(b => b.AutoTagOverride("Product").WithSummary("获取产品类型详情（用于编辑回填）"));
        Get("/api/admin/product-types/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProductView);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = new ProductTypeId(Route<Guid>("id"));
        var result = await query.GetByIdAsync(id, ct);
        if (result == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
