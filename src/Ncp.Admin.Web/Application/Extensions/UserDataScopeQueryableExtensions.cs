using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;
using Ncp.Admin.Infrastructure.Services;

namespace Ncp.Admin.Web.Application.Extensions;

/// <summary>
/// 按 JWT 数据权限筛选用户查询（部门类范围通过 <c>user_dept</c> 归属判断）。
/// </summary>
public static class UserDataScopeQueryableExtensions
{
    public static IQueryable<User> ApplyUserDataScope(
        this IQueryable<User> query,
        ApplicationDbContext dbContext,
        DataPermissionContext ctx) =>
        ctx.Scope switch
        {
            DataScope.All => query,
            DataScope.Self => ctx.UserId != null
                ? query.Where(u => u.Id == ctx.UserId)
                : query.Where(_ => false),
            DataScope.Dept => ctx.DeptId is { } singleDept
                ? query.FilterUsersByDeptIds(dbContext, [singleDept])
                : query.Where(_ => false),
            DataScope.DeptAndSub or DataScope.CustomDeptAndSub => ctx.AuthorizedDeptIds.Count > 0
                ? query.FilterUsersByDeptIds(dbContext, ctx.AuthorizedDeptIds)
                : query.Where(_ => false),
            _ => query,
        };

    /// <summary>
    /// 用户主部门记录在 <see cref="UserDept"/>（与用户一对一），与统计「应提交人数」按部门展开口径一致。
    /// </summary>
    public static IQueryable<User> FilterUsersByDeptIds(
        this IQueryable<User> users,
        ApplicationDbContext dbContext,
        IReadOnlyList<DeptId> deptIds)
    {
        if (deptIds.Count == 0)
            return users.Where(_ => false);

        return users.Where(u =>
            dbContext.UserDepts.Any(ud => ud.Id == u.Id && deptIds.Contains(ud.DeptId)));
    }
}
