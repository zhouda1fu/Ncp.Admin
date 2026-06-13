using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Services;

/// <summary>
/// 从服务端加载当前用户权限码，并附加到本次请求的用户主体中，
/// 避免访问令牌长期携带完整权限列表。
/// </summary>
public sealed class PermissionClaimsTransformation(
    UserQuery userQuery,
    RoleQuery roleQuery,
    IMemoryCache memoryCache) : IClaimsTransformation
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(1);
    public static string GetPermissionCodesCacheKey(UserId userId) => $"auth:permission-codes:{userId.Id}";

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            return principal;
        }

        var identity = principal.Identity as ClaimsIdentity;
        if (identity == null || identity.HasClaim(c => c.Type == JwtPermissionClaimTypes.Permissions))
        {
            return principal;
        }

        if (!principal.TryGetUserId(out var userId))
        {
            return principal;
        }

        var permissionCodes = await GetPermissionCodesAsync(userId, CancellationToken.None);
        if (permissionCodes.Count == 0)
        {
            return principal;
        }

        identity.AddClaims(permissionCodes.Select(code => new Claim(JwtPermissionClaimTypes.Permissions, code)));
        return principal;
    }

    private async Task<IReadOnlyList<string>> GetPermissionCodesAsync(
        UserId userId,
        CancellationToken cancellationToken)
    {
        var cacheKey = GetPermissionCodesCacheKey(userId);
        return await memoryCache.GetOrCreateAsync(cacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = CacheDuration;

            var userInfo = await userQuery.GetUserInfoForLoginByIdAsync(userId, cancellationToken);
            if (userInfo == null)
            {
                return Array.Empty<string>();
            }

            var roleIds = userInfo.UserRoles.Select(r => r.RoleId).Distinct().ToList();
            if (roleIds.Count == 0)
            {
                return Array.Empty<string>();
            }

            var permissions = await roleQuery.GetAssignedPermissionCodesAsync(roleIds, cancellationToken);
            return permissions.Distinct(StringComparer.Ordinal).ToArray();
        }) ?? Array.Empty<string>();
    }
}
