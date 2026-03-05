using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Product;

/// <summary>
/// 获取产品列表请求（用于订单明细产品下拉等）
/// </summary>
public class GetProductListRequest : ProductQueryInput { }

/// <summary>
/// 获取产品分页列表
/// </summary>
public class GetProductListEndpoint(ProductQuery query)
    : Endpoint<GetProductListRequest, ResponseData<PagedData<ProductQueryDto>>>
{
    public override void Configure()
    {
        Tags("Product");
        Get("/api/admin/products");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrderView, PermissionCodes.ProductView);
    }

    public override async Task HandleAsync(GetProductListRequest req, CancellationToken ct)
    {
        var result = await query.GetPagedAsync(req, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
