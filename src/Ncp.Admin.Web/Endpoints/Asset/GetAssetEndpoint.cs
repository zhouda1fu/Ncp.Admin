using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.AssetAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Asset;

public record GetAssetRequest(AssetId Id);

public class GetAssetEndpoint(AssetQuery query) : Endpoint<GetAssetRequest, ResponseData<AssetQueryDto>>
{
    public override void Configure()
    {
        Tags("Asset");
        Get("/api/admin/assets/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.AssetView);
    }

    public override async Task HandleAsync(GetAssetRequest req, CancellationToken ct)
    {
        var result = await query.GetByIdAsync(req.Id, ct);
        if (result == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
