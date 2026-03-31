using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.PositionAggregate;
using Ncp.Admin.Web.Application.Commands.Identity.Admin.PositionCommands;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Identity.Admin.PositionEndpoints;

/// <summary>
/// 停用岗位的请求模型
/// </summary>
/// <param name="PositionId">要停用的岗位ID</param>
public record DeactivatePositionRequest(PositionId PositionId);

/// <summary>
/// 停用岗位
/// </summary>
/// <param name="mediator">MediatR 中介者</param>
public class DeactivatePositionEndpoint(IMediator mediator) : Endpoint<DeactivatePositionRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Positions");
        Description(b => b.AutoTagOverride("Positions"));
        Put("/api/admin/position/deactivate");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.PositionEdit);
    }

    public override async Task HandleAsync(DeactivatePositionRequest req, CancellationToken ct)
    {
        var cmd = new DeactivatePositionCommand(req.PositionId);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
