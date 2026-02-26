using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.AssetAggregate;
using Ncp.Admin.Web.Application.Commands.Asset;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Asset;

public class ScrapAssetRequest
{
    public Guid Id { get; set; }
}

public class ScrapAssetEndpoint(IMediator mediator) : Endpoint<ScrapAssetRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Asset");
        Post("/api/admin/assets/{id}/scrap");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.AssetScrap);
    }

    public override async Task HandleAsync(ScrapAssetRequest req, CancellationToken ct)
    {
        var cmd = new ScrapAssetCommand(new AssetId(req.Id));
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
