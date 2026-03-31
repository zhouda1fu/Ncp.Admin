using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Product;

/// <summary>
/// 获取某产品的参数列表
/// </summary>
public class GetProductParameterListEndpoint(ProductParameterQuery query)
    : EndpointWithoutRequest<ResponseData<IReadOnlyList<ProductParameterDto>>>
{
    public override void Configure()
    {
        Tags("Product");
        Get("/api/admin/products/{productId}/parameters");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProductView);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var productId = new ProductId(Route<Guid>("productId"));
        var list = await query.GetByProductIdAsync(productId, ct);
        await Send.OkAsync(list.AsResponseData(), cancellation: ct);
    }
}
