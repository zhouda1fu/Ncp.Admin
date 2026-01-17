using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Web.Application.Commands.Identity.Admin.RoleCommands;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Identity.Admin.RoleEndpoints;

/// <summary>
/// 激活角色的请求模型
/// </summary>
/// <param name="RoleId">要激活的角色ID</param>
public record ActivateRoleRequest(RoleId RoleId);

/// <summary>
/// 激活角色的API端点
/// 该端点用于激活已停用的角色
/// </summary>
[Tags("Roles")]
public class ActivateRoleEndpoint(IMediator mediator) : Endpoint<ActivateRoleRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Put("/api/admin/roles/activate");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.RoleEdit);
    }

    public override async Task HandleAsync(ActivateRoleRequest req, CancellationToken ct)
    {
        var cmd = new ActivateRoleCommand(req.RoleId);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
