using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.RegionAggregate;
using Ncp.Admin.Web.Application.Commands.Region;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Region;

public class CreateRegionRequest
{
    public long Code { get; set; }
    public string Name { get; set; } = "";
    public long ParentCode { get; set; }
    public int Level { get; set; }
    public int SortOrder { get; set; }
}

public record CreateRegionResponse(RegionId Id);

public class CreateRegionEndpoint(IMediator mediator)
    : Endpoint<CreateRegionRequest, ResponseData<CreateRegionResponse>>
{
    public override void Configure()
    {
        Tags("Region");
        Post("/api/admin/regions");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.RegionCreate);
    }

    public override async Task HandleAsync(CreateRegionRequest req, CancellationToken ct)
    {
        var cmd = new CreateRegionCommand(req.Code, req.Name, req.ParentCode, req.Level, req.SortOrder);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateRegionResponse(id).AsResponseData(), cancellation: ct);
    }
}
