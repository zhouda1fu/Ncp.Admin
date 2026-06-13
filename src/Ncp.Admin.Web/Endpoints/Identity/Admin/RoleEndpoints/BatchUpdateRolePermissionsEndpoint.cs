using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Web.Application.Commands.Identity.Admin.RoleCommands;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Identity.Admin.RoleEndpoints;

/// <summary>
/// 批量调整角色权限请求。
/// </summary>
/// <param name="RoleIds">角色标识列表。</param>
/// <param name="Operation">操作类型。</param>
/// <param name="PermissionCodes">权限码列表。</param>
public record BatchUpdateRolePermissionsRequest(
    IReadOnlyList<RoleId> RoleIds,
    RolePermissionBatchOperation Operation,
    IReadOnlyList<string> PermissionCodes);

/// <summary>
/// 批量调整角色权限。
/// </summary>
public class BatchUpdateRolePermissionsEndpoint(IMediator mediator)
    : Endpoint<BatchUpdateRolePermissionsRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Roles");
        Description(b => b.AutoTagOverride("Roles").WithSummary("批量调整角色权限"));
        Put("/api/admin/roles/permissions/batch");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.RoleUpdatePermissions);
    }

    public override async Task HandleAsync(BatchUpdateRolePermissionsRequest req, CancellationToken ct)
    {
        await mediator.Send(
            new BatchUpdateRolePermissionsCommand(req.RoleIds, req.Operation, req.PermissionCodes),
            ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
