using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Region;

public class GetRegionListEndpoint(RegionQuery query)
    : EndpointWithoutRequest<ResponseData<IReadOnlyList<RegionDto>>>
{
    public override void Configure()
    {
        Tags("Region");
        Get("/api/admin/regions");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.RegionView);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await query.GetListAsync(ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
