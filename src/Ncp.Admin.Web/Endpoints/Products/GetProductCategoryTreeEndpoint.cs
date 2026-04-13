using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Products;

/// <summary>
/// 获取产品分类树请求
/// </summary>
/// <param name="IncludeInvisible">是否包含不可见分类</param>
public record GetProductCategoryTreeRequest(bool IncludeInvisible = false);

/// <summary>
/// 获取产品分类树（用于产品表单下拉等）
/// </summary>
public class GetProductCategoryTreeEndpoint(ProductCategoryQuery query)
    : Endpoint<GetProductCategoryTreeRequest, ResponseData<IEnumerable<ProductCategoryTreeDto>>>
{
    public override void Configure()
    {
        Tags("Product");
        Description(b => b.AutoTagOverride("Product").WithSummary("获取产品分类树（用于产品表单下拉等）"));
        Get("/api/admin/product-categories/tree");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProductView);
    }

    public override async Task HandleAsync(GetProductCategoryTreeRequest req, CancellationToken ct)
    {
        var tree = await query.GetTreeAsync(req.IncludeInvisible, ct);
        await Send.OkAsync(tree.AsResponseData(), cancellation: ct);
    }
}
