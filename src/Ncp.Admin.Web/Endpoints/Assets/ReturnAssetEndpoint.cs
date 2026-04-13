using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.AssetAggregate;
using Ncp.Admin.Web.Application.Commands.Assets;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Assets;

/// <summary>
/// 归还资产请求
/// </summary>
/// <param name="AllocationId">资产分配 ID</param>
public record ReturnAssetRequest(AssetAllocationId AllocationId);

public class ReturnAssetEndpoint(IMediator mediator) : Endpoint<ReturnAssetRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Asset");
        Description(b => b.AutoTagOverride("Asset").WithSummary("归还资产请求"));
        Post("/api/admin/asset-allocations/{allocationId}/return");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.AssetReturn);
    }

    public override async Task HandleAsync(ReturnAssetRequest req, CancellationToken ct)
    {
        var cmd = new ReturnAssetCommand(req.AllocationId);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
