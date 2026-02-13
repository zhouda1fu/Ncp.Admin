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

        var userIdClaim = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdClaim) || !long.TryParse(userIdClaim, out var userIdValue))
            return null;

        var userId = new UserId(userIdValue);

        var scope = DataScope.All;
        var dataScopeClaim = user.FindFirstValue("data_scope");
        if (!string.IsNullOrEmpty(dataScopeClaim) && int.TryParse(dataScopeClaim, out var scopeValue) &&
            scopeValue >= (int)DataScope.All && scopeValue <= (int)DataScope.Self)
            scope = (DataScope)scopeValue;

        DeptId? deptId = null;
        var deptIdClaim = user.FindFirstValue("dept_id");
        if (!string.IsNullOrEmpty(deptIdClaim) && long.TryParse(deptIdClaim, out var deptIdValue))
            deptId = new DeptId(deptIdValue);

        IReadOnlyList<DeptId> authorizedDeptIds;
        var authorizedDeptIdsClaim = user.FindFirstValue("authorized_dept_ids");
        if (!string.IsNullOrEmpty(authorizedDeptIdsClaim))
        {
            authorizedDeptIds = authorizedDeptIdsClaim
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Where(s => long.TryParse(s.Trim(), out _))
                .Select(s => new DeptId(long.Parse(s.Trim())))
                .ToList();
        }
        else
        {
            authorizedDeptIds = deptId != null ? [deptId] : [];
        }

        return new DataPermissionContext(scope, userId, deptId, authorizedDeptIds);
    }
}
