using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.AssetAggregate;
using Ncp.Admin.Web.Application.Commands.Asset;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Asset;

public class ReturnAssetRequest
{
    public Guid AllocationId { get; set; }
}

public class ReturnAssetEndpoint(IMediator mediator) : Endpoint<ReturnAssetRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Asset");
        Post("/api/admin/asset-allocations/{allocationId}/return");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.AssetReturn);
    }

    public override async Task HandleAsync(ReturnAssetRequest req, CancellationToken ct)
    {
        var cmd = new ReturnAssetCommand(new AssetAllocationId(req.AllocationId));
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
