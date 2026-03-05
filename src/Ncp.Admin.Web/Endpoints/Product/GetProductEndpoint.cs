using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Product;

/// <summary>
/// 获取产品详情
/// </summary>
public class GetProductEndpoint(ProductQuery query) : EndpointWithoutRequest<ResponseData<ProductQueryDto>>
{
    public override void Configure()
    {
        Tags("Product");
        Get("/api/admin/products/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProductView);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = new ProductId(Route<Guid>("id"));
        var result = await query.GetByIdAsync(id, ct);
        if (result == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
