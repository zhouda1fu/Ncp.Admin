namespace Ncp.Admin.Web.Services;

/// <summary>
/// JWT 中与数据权限上下文（Infrastructure 中的 <c>DataPermissionContext</c>）对应的自定义 claim 类型（签发与解析须一致）。
/// </summary>
public static class JwtDataPermissionClaimTypes
{
    public const string DataScope = "data_scope";
    public const string DeptId = "dept_id";
    public const string AuthorizedDeptIds = "authorized_dept_ids";
}
