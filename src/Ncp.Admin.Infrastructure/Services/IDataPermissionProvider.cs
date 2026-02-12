using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Infrastructure.Services;

/// <summary>
/// 数据权限上下文提供者。
/// 返回当前用户的数据权限范围（最高 DataScope）、部门 ID、用户 ID，供全局查询过滤使用。
/// 需在请求早期（如中间件）调用 <see cref="LoadAsync"/> 完成初始化。
/// </summary>
public interface IDataPermissionProvider
{
    /// <summary>
    /// 加载当前用户的数据权限上下文（从 HTTP 上下文解析用户，并查询其角色与部门）。
    /// 应在每个请求早期调用一次。
    /// </summary>
    Task LoadAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// 当前用户的数据权限范围（取用户所有角色中最宽松的 DataScope）。
    /// </summary>
    DataScope Scope { get; }

    /// <summary>
    /// 当前用户 ID；未认证时为 null。
    /// </summary>
    UserId? UserId { get; }

    /// <summary>
    /// 当前用户所属部门 ID；无部门时为 null。
    /// </summary>
    DeptId? DeptId { get; }

    /// <summary>
    /// 当前用户有权访问的部门 ID 列表（用于 DeptAndSub 模式，包含本部门及所有子部门）。
    /// </summary>
    IReadOnlyList<DeptId> AuthorizedDeptIds { get; }
}
