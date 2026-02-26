using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.AssetAggregate;
using Ncp.Admin.Web.Application.Commands.Asset;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Asset;

public class UpdateAssetRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public string Code { get; set; } = "";
    public DateTimeOffset PurchaseDate { get; set; }
    public decimal Value { get; set; }
    public string? Remark { get; set; }
}

public class UpdateAssetEndpoint(IMediator mediator) : Endpoint<UpdateAssetRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Asset");
        Put("/api/admin/assets/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.AssetEdit);
    }

    public override async Task HandleAsync(UpdateAssetRequest req, CancellationToken ct)
    {
        var cmd = new UpdateAssetCommand(new AssetId(req.Id), req.Name, req.Category, req.Code, req.PurchaseDate, req.Value, req.Remark);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
