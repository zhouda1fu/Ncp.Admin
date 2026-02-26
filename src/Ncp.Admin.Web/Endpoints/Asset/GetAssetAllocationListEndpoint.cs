using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Asset;

public class GetAssetAllocationListRequest : AssetAllocationQueryInput { }

public class GetAssetAllocationListEndpoint(AssetAllocationQuery query)
    : Endpoint<GetAssetAllocationListRequest, ResponseData<PagedData<AssetAllocationQueryDto>>>
{
    public override void Configure()
    {
        Tags("Asset");
        Get("/api/admin/asset-allocations");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.AssetAllocationView);
    }

    public override async Task HandleAsync(GetAssetAllocationListRequest req, CancellationToken ct)
    {
        var result = await query.GetPagedAsync(req, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
