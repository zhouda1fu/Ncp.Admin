using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Assets;

/// <summary>
/// 资产领用列表请求
/// </summary>
/// <param name="PageIndex">页码</param>
/// <param name="PageSize">每页条数</param>
/// <param name="AssetId">资产 ID</param>
/// <param name="UserId">领用人用户 ID</param>
/// <param name="Returned">是否已归还</param>
public record GetAssetAllocationListRequest(
    int PageIndex = 1,
    int PageSize = 20,
    Guid? AssetId = null,
    long? UserId = null,
    bool? Returned = null);

public class GetAssetAllocationListEndpoint(AssetAllocationQuery query)
    : Endpoint<GetAssetAllocationListRequest, ResponseData<PagedData<AssetAllocationQueryDto>>>
{
    public override void Configure()
    {
        Tags("Asset");
        Description(b => b.AutoTagOverride("Asset").WithSummary("资产领用列表请求"));
        Get("/api/admin/asset-allocations");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.AssetAllocationView);
    }

    public override async Task HandleAsync(GetAssetAllocationListRequest req, CancellationToken ct)
    {
        var input = new AssetAllocationQueryInput
        {
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            AssetId = req.AssetId,
            UserId = req.UserId,
            Returned = req.Returned,
        };
        var result = await query.GetPagedAsync(input, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
