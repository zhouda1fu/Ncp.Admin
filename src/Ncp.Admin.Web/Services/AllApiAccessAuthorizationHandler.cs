using Microsoft.AspNetCore.Authorization;
using Ncp.Admin.Web.AppPermissions;
using Ncp.Admin.Web.Extensions;

namespace Ncp.Admin.Web.Services;

/// <summary>
/// 超级管理员 <see cref="PermissionCodes.AllApiAccess"/> 全局兜底：满足任意权限策略要求。
/// </summary>
public sealed class AllApiAccessAuthorizationHandler : AuthorizationHandler<IAuthorizationRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        IAuthorizationRequirement requirement)
    {
        if (context.User.GetAppPermissionCodes().Contains(PermissionCodes.AllApiAccess))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
