using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.AssetAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Asset;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Asset;

/// <summary>
/// 分配资产请求
/// </summary>
/// <param name="AssetId">资产 ID</param>
/// <param name="UserId">使用人用户 ID</param>
/// <param name="Note">备注</param>
public record AllocateAssetRequest(AssetId AssetId, UserId UserId, string? Note);

public class AllocateAssetEndpoint(IMediator mediator) : Endpoint<AllocateAssetRequest, ResponseData<AllocateAssetResponse>>
{
    public override void Configure()
    {
        Tags("Asset");
        Post("/api/admin/assets/{assetId}/allocate");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.AssetAllocate);
    }

    public override async Task HandleAsync(AllocateAssetRequest req, CancellationToken ct)
    {
        var cmd = new AllocateAssetCommand(req.AssetId, req.UserId, req.Note);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new AllocateAssetResponse(id).AsResponseData(), cancellation: ct);
    }
}

public record AllocateAssetResponse(AssetAllocationId Id);
