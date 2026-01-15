using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Web.Application.Commands.UserCommands;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.UserEndpoints;

/// <summary>
/// 更新用户角色的请求模型
/// </summary>
/// <param name="UserId">要更新角色的用户ID</param>
/// <param name="RoleIds">要分配给用户的角色ID列表</param>
public record UpdateUserRolesRequest(UserId UserId, IEnumerable<RoleId> RoleIds);

/// <summary>
/// 更新用户角色的响应模型
/// </summary>
/// <param name="UserId">已更新角色的用户ID</param>
public record UpdateUserRolesResponse(UserId UserId);

/// <summary>
/// 更新用户角色的API端点
/// 该端点用于修改指定用户的角色分配，支持批量角色分配
/// </summary>
[Tags("Users")]
public class UpdateUserRolesEndpoint(IMediator mediator, RoleQuery roleQuery) : Endpoint<UpdateUserRolesRequest, ResponseData<UpdateUserRolesResponse>>
{
    public override void Configure()
    {
        Put("/api/users/update-roles");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.UserRoleAssign);
    }

    public override async Task HandleAsync(UpdateUserRolesRequest request, CancellationToken ct)
    {
        var rolesToBeAssigned = await roleQuery.GetAdminRolesForAssignmentAsync(request.RoleIds, ct);
        var cmd = new UpdateUserRolesCommand(request.UserId, rolesToBeAssigned);
        await mediator.Send(cmd, ct);
        var response = new UpdateUserRolesResponse(request.UserId);
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}

