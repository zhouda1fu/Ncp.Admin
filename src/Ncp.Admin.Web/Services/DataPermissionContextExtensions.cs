using System.Security.Claims;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Services;

namespace Ncp.Admin.Web.Services;

/// <summary>
/// 从 ClaimsPrincipal 解析 DataPermissionContext 的扩展方法。
/// </summary>
public static class DataPermissionContextExtensions
{
    /// <summary>
    /// 从当前用户的 claims 解析数据权限上下文。
    /// 未认证或解析失败时返回 null。
    /// </summary>
    public static DataPermissionContext? ParseFromClaims(this ClaimsPrincipal? user)
    {
        if (user == null)
            return null;

        if (!user.TryGetUserId(out var userId))
            return null;

        var scope = DataScope.All;
        var dataScopeClaim = user.FindFirstValue(JwtDataPermissionClaimTypes.DataScope);
        if (!string.IsNullOrWhiteSpace(dataScopeClaim) && int.TryParse(dataScopeClaim.Trim(), out var scopeValue) &&
            scopeValue >= (int)DataScope.All && scopeValue <= (int)DataScope.CustomDeptAndSub)
            scope = (DataScope)scopeValue;

        DeptId? deptId = null;
        var deptIdClaim = user.FindFirstValue(JwtDataPermissionClaimTypes.DeptId);
        if (!string.IsNullOrWhiteSpace(deptIdClaim) && long.TryParse(deptIdClaim.Trim(), out var deptIdValue))
            deptId = new DeptId(deptIdValue);

        IReadOnlyList<DeptId> authorizedDeptIds;
        var authorizedDeptIdsClaim = user.FindFirstValue(JwtDataPermissionClaimTypes.AuthorizedDeptIds);
        // 仅含空白视为未提供，与无 claim 一致走 deptId 兜底，避免解析出空列表
        if (!string.IsNullOrWhiteSpace(authorizedDeptIdsClaim))
        {
            var parsed = new List<DeptId>();
            foreach (var segment in authorizedDeptIdsClaim.Split(
                         ',',
                         StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (long.TryParse(segment, out var id))
                    parsed.Add(new DeptId(id));
            }

            authorizedDeptIds = parsed;
        }
        else
        {
            authorizedDeptIds = deptId != null ? [deptId] : [];
        }

        return new DataPermissionContext(scope, userId, deptId, authorizedDeptIds);
    }
}
