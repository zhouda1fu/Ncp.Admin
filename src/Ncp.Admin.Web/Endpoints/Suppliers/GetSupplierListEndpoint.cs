using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Suppliers;

/// <summary>
/// 获取供应商列表请求
/// </summary>
/// <param name="Keyword">关键词（名称/简称/联系人）</param>
public record GetSupplierListRequest(string? Keyword);

/// <summary>
/// 获取供应商列表（用于产品表单下拉等）
/// </summary>
public class GetSupplierListEndpoint(SupplierQuery query)
    : Endpoint<GetSupplierListRequest, ResponseData<IReadOnlyList<SupplierDto>>>
{
    public override void Configure()
    {
        Tags("Supplier");
        Description(b => b.AutoTagOverride("Supplier").WithSummary("获取供应商列表（用于产品表单下拉等）"));
        Get("/api/admin/suppliers");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProductView);
    }

    public override async Task HandleAsync(GetSupplierListRequest req, CancellationToken ct)
    {
        var list = await query.GetListAsync(req.Keyword, ct);
        await Send.OkAsync(list.AsResponseData(), cancellation: ct);
    }
}
