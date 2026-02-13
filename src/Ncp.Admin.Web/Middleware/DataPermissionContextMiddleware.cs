using Ncp.Admin.Infrastructure.Services;
using Ncp.Admin.Web.Services;
using NetCorePal.Context;

namespace Ncp.Admin.Web.Middleware;

/// <summary>
/// 从当前请求的 JWT claims 解析数据权限上下文并写入 NetCorePal <see cref="IContextAccessor"/>。
/// 若上下文中已存在（如从 HttpClient/CAP 传递），则不再覆盖。
/// 需在 UseAuthentication、UseContext 之后注册。
/// </summary>
public sealed class DataPermissionContextMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, IContextAccessor accessor)
    {
        if (accessor.GetContext<DataPermissionContext>() == null)
            accessor.SetContext(context.User.ParseFromClaims());
        await next(context);
    }
}
