using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.PositionAggregate;
using Ncp.Admin.Web.Application.Commands.Identity.Admin.PositionCommands;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Identity.Admin.PositionEndpoints;

/// <summary>
/// 删除岗位的请求模型
/// </summary>
public record DeletePositionRequest(PositionId Id);

/// <summary>
/// 删除岗位
/// </summary>
public class DeletePositionEndpoint(IMediator mediator) : Endpoint<DeletePositionRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Positions");
        Description(b => b.AutoTagOverride("Positions"));
        Delete("/api/admin/position/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.PositionDelete);
    }

    public override async Task HandleAsync(DeletePositionRequest request, CancellationToken ct)
    {
        await mediator.Send(new DeletePositionCommand(request.Id), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
