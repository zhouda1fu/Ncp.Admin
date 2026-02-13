using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Infrastructure.Services;

/// <summary>
/// 数据权限上下文，承载当前用户的数据权限范围、部门、用户 ID 等，供全局查询过滤使用。
/// 与 NetCorePal 的 Context Passing 模式一致，支持 HttpClient、CAP 等场景的跨服务传递。
/// 参见 <see href="https://netcorepal.github.io/netcorepal-cloud-framework/zh/context/custom-context-type/"/>
/// </summary>
/// <param name="Scope">数据权限范围</param>
/// <param name="UserId">用户 ID，未认证时为 null</param>
/// <param name="DeptId">部门 ID，无部门时为 null</param>
/// <param name="AuthorizedDeptIds">有权访问的部门 ID 列表（DeptAndSub 模式，含本部门及子部门）</param>
public sealed record DataPermissionContext(
    DataScope Scope,
    UserId? UserId,
    DeptId? DeptId,
    IReadOnlyList<DeptId> AuthorizedDeptIds)
{
    /// <summary>
    /// 上下文在 Carrier/Source（如 HTTP 头）中存储的 Key。
    /// </summary>
    public static string ContextKey { get; } = "x-data-permission-context";
}
