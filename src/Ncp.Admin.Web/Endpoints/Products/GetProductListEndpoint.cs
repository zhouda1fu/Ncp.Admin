using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Products;

/// <summary>
/// 获取产品列表请求（用于订单明细产品下拉等）
/// </summary>
/// <param name="PageIndex">页码</param>
/// <param name="PageSize">每页条数</param>
/// <param name="Keyword">关键字</param>
public record GetProductListRequest(
    int PageIndex = 1,
    int PageSize = 20,
    string? Keyword = null);

/// <summary>
/// 获取产品分页列表
/// </summary>
public class GetProductListEndpoint(ProductQuery query)
    : Endpoint<GetProductListRequest, ResponseData<PagedData<ProductQueryDto>>>
{
    public override void Configure()
    {
        Tags("Product");
        Description(b => b.AutoTagOverride("Product").WithSummary("获取产品分页列表"));
        Get("/api/admin/products");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrderView, PermissionCodes.ProductView);
    }

    public override async Task HandleAsync(GetProductListRequest req, CancellationToken ct)
    {
        var input = new ProductQueryInput
        {
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            Keyword = req.Keyword,
        };
        var result = await query.GetPagedAsync(input, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
