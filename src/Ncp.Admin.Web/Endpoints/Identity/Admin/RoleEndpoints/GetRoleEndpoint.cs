using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Identity.Admin.RoleEndpoints;

/// <summary>
/// 获取角色信息的请求模型
/// </summary>
/// <param name="Id">要查询的角色ID</param>
public record GetRoleRequest(RoleId Id);

/// <summary>
/// 获取角色
/// </summary>
/// <param name="roleQuery"></param>
public class GetRoleEndpoint(RoleQuery roleQuery) : Endpoint<GetRoleRequest, ResponseData<RoleQueryDto>>
{
    public override void Configure()
    {
        Tags("Roles");
        Description(b => b.AutoTagOverride("Roles"));
        Get("/api/admin/roles/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.RoleView);
    }

    public override async Task HandleAsync(GetRoleRequest req, CancellationToken ct)
    {
        var roleInfo = await roleQuery.GetRoleByIdAsync(req.Id, ct);
        if (roleInfo == null)
            await Send.NotFoundAsync(ct);
        else
            await Send.OkAsync(roleInfo.AsResponseData(), cancellation: ct);
    }
}

