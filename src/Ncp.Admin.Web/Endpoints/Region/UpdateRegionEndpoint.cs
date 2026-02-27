using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Commands.Region;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Region;

public class UpdateRegionRequest
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
    public long ParentCode { get; set; }
    public int Level { get; set; }
    public int SortOrder { get; set; }
}

public class UpdateRegionEndpoint(IMediator mediator)
    : Endpoint<UpdateRegionRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Region");
        Put("/api/admin/regions/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.RegionEdit);
    }

    public override async Task HandleAsync(UpdateRegionRequest req, CancellationToken ct)
    {
        var cmd = new UpdateRegionCommand(req.Id, req.Name, req.ParentCode, req.Level, req.SortOrder);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
