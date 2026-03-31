using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.PositionAggregate;
using Ncp.Admin.Web.Application.Commands.Identity.Admin.PositionCommands;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Identity.Admin.PositionEndpoints;

/// <summary>
/// 激活岗位的请求模型
/// </summary>
/// <param name="PositionId">要激活的岗位ID</param>
public record ActivatePositionRequest(PositionId PositionId);

/// <summary>
/// 激活岗位
/// </summary>
/// <param name="mediator">MediatR 中介者</param>
public class ActivatePositionEndpoint(IMediator mediator) : Endpoint<ActivatePositionRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Positions");
        Description(b => b.AutoTagOverride("Positions"));
        Put("/api/admin/position/activate");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.PositionEdit);
    }

    public override async Task HandleAsync(ActivatePositionRequest req, CancellationToken ct)
    {
        var cmd = new ActivatePositionCommand(req.PositionId);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
