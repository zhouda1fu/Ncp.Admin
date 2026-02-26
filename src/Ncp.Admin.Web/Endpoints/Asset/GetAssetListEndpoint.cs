using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Asset;

public class GetAssetListRequest : AssetQueryInput { }

public class GetAssetListEndpoint(AssetQuery query)
    : Endpoint<GetAssetListRequest, ResponseData<PagedData<AssetQueryDto>>>
{
    public override void Configure()
    {
        Tags("Asset");
        Get("/api/admin/assets");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.AssetView);
    }

    public override async Task HandleAsync(GetAssetListRequest req, CancellationToken ct)
    {
        var result = await query.GetPagedAsync(req, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
