using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Products;

/// <summary>
/// 获取产品类型列表请求（用于下拉、产品表单）
/// </summary>
/// <param name="IncludeInvisible">是否包含不可见类型</param>
public record GetProductTypeListRequest(bool IncludeInvisible = false);

/// <summary>
/// 获取产品类型列表
/// </summary>
public class GetProductTypeListEndpoint(ProductTypeQuery query)
    : Endpoint<GetProductTypeListRequest, ResponseData<IEnumerable<ProductTypeDto>>>
{
    public override void Configure()
    {
        Tags("Product");
        Description(b => b.AutoTagOverride("Product").WithSummary("获取产品类型列表"));
        Get("/api/admin/product-types");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProductView);
    }

    public override async Task HandleAsync(GetProductTypeListRequest req, CancellationToken ct)
    {
        var list = await query.GetListAsync(req.IncludeInvisible, ct);
        await Send.OkAsync(list.AsResponseData(), cancellation: ct);
    }
}
