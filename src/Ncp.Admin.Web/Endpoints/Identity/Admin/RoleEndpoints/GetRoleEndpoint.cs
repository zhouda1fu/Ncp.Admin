using FastEndpoints;
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
/// 获取角色信息的API端点
/// 该端点用于根据角色ID查询角色的详细信息，包括权限列表
/// </summary>
[Tags("Roles")]
public class GetRoleEndpoint(RoleQuery roleQuery) : Endpoint<GetRoleRequest, ResponseData<RoleQueryDto?>>
{
  
    public override void Configure()
    {
        Get("/api/admin/roles/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.RoleView);
    }

   
    public override async Task HandleAsync(GetRoleRequest req, CancellationToken ct)
    {
        // 通过查询服务获取角色详细信息
        var roleInfo = await roleQuery.GetRoleByIdAsync(req.Id, ct);

        // 验证角色是否存在
        if (roleInfo == null)
        {
            throw new KnownException($"未找到角色，Id = {req.Id}");
        }
        await Send.OkAsync(roleInfo!.AsResponseData(), cancellation: ct);
    }
}

