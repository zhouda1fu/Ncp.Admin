using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Web.Application.Commands.Identity.Admin.RoleCommands;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Identity.Admin.RoleEndpoints;

/// <summary>
/// 更新角色信息的请求模型
/// </summary>
/// <param name="RoleId">要更新的角色ID</param>
/// <param name="Name">新的角色名称</param>
/// <param name="Description">新的角色描述</param>
/// <param name="PermissionCodes">新的权限代码列表</param>
/// <param name="CustomDeptIds">自定义部门ID列表（当 DataScope=CustomDeptAndSub 时必填）</param>
public record UpdateRoleInfoRequest(
    RoleId RoleId,
    string Name,
    string Description,
    DataScope? DataScope,
    IEnumerable<string> PermissionCodes,
    IEnumerable<DeptId>? CustomDeptIds = null);

/// <summary>
/// 更新角色
/// </summary>
/// <param name="mediator"></param>
public class UpdateRoleEndpoint(IMediator mediator) : Endpoint<UpdateRoleInfoRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Roles");
        Description(b => b.AutoTagOverride("Roles").WithSummary("更新角色"));
        Put("/api/admin/roles/update");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.RoleEdit);
    }

    public override async Task HandleAsync(UpdateRoleInfoRequest request, CancellationToken ct)
    {
        var cmd = new UpdateRoleInfoCommand(
            request.RoleId,
            request.Name,
            request.Description,
            request.DataScope,
            request.PermissionCodes,
            request.CustomDeptIds);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}

