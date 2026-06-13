using System.Security.Claims;
using Ncp.Admin.Web.AppPermissions;
using Ncp.Admin.Web.Services;

namespace Ncp.Admin.Web.Extensions;

/// <summary>
/// JWT 权限 claim（<see cref="JwtPermissionClaimTypes.Permissions"/>）判断扩展。
/// </summary>
public static class ClaimsPrincipalPermissionExtensions
{
    /// <summary>读取当前用户 JWT 中的全部应用权限码。</summary>
    public static HashSet<string> GetAppPermissionCodes(this ClaimsPrincipal user) =>
        user.Claims
            .Where(c => c.Type == JwtPermissionClaimTypes.Permissions)
            .Select(c => c.Value)
            .ToHashSet(StringComparer.Ordinal);

    /// <summary>当前用户是否拥有任一指定权限码；持有 <see cref="PermissionCodes.AllApiAccess"/> 时视为全部放行。</summary>
    public static bool HasAnyAppPermission(this ClaimsPrincipal user, params string[] permissionCodes)
    {
        if (permissionCodes is not { Length: > 0 })
        {
            return false;
        }

        var granted = user.GetAppPermissionCodes();
        if (granted.Contains(PermissionCodes.AllApiAccess))
        {
            return true;
        }

        foreach (var code in permissionCodes)
        {
            if (granted.Contains(code))
            {
                return true;
            }
        }

        return false;
    }
}
