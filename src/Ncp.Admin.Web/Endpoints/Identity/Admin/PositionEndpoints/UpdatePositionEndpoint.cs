using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.PositionAggregate;
using Ncp.Admin.Web.Application.Commands.Identity.Admin.PositionCommands;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Identity.Admin.PositionEndpoints;

/// <summary>
/// 更新岗位的请求模型
/// </summary>
public record UpdatePositionRequest(PositionId Id, string Name, string Code, string Description, DeptId DeptId, int SortOrder, int Status);

/// <summary>
/// 更新岗位
/// </summary>
public class UpdatePositionEndpoint(IMediator mediator) : Endpoint<UpdatePositionRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Positions");
        Description(b => b.AutoTagOverride("Positions"));
        Put("/api/admin/position");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.PositionEdit);
    }

    public override async Task HandleAsync(UpdatePositionRequest req, CancellationToken ct)
    {
        var command = new UpdatePositionCommand(req.Id, req.Name, req.Code, req.Description, req.DeptId, req.SortOrder, req.Status);
        await mediator.Send(command, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
