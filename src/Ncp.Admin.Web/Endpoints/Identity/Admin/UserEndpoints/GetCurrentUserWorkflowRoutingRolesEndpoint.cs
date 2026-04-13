using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Identity.Admin.UserEndpoints;

/// <summary>
/// 当前用户可用于工作流路由的角色项（作废审批等多角色弹窗）
/// </summary>
/// <param name="RoleId">角色 ID</param>
/// <param name="RoleName">角色名称</param>
public record WorkflowRoutingRoleItem(RoleId RoleId, string RoleName);

/// <summary>
/// 获取当前登录用户的工作流路由角色列表
/// </summary>
public class GetCurrentUserWorkflowRoutingRolesEndpoint(UserQuery userQuery)
    : EndpointWithoutRequest<ResponseData<List<WorkflowRoutingRoleItem>>>
{
    public override void Configure()
    {
        Tags("Users");
        Description(b => b.AutoTagOverride("Users").WithSummary("获取当前用户工作流路由角色列表"));
        Get("/api/admin/user/current/workflow-routing-roles");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerSeaVoid);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!User.TryGetUserId(out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var loginInfo = await userQuery.GetUserInfoForLoginByIdAsync(userId, ct);
        if (loginInfo is null)
        {
            throw new KnownException("无效的用户", ErrorCodes.InvalidUser);
        }

        var items = loginInfo.UserRoles
            .OrderBy(r => r.RoleName, StringComparer.Ordinal)
            .Select(r => new WorkflowRoutingRoleItem(r.RoleId, r.RoleName))
            .ToList();

        await Send.OkAsync(items.AsResponseData(), cancellation: ct);
    }
}
