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
        // HTTP 请求始终以当前 JWT 为准，避免沿用 HttpClient/CAP 传入的上下文导致数据权限失效。
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var parsed = context.User.ParseFromClaims();
            if (parsed != null)
                accessor.SetContext(parsed);
        }

        await next(context);
    }
}
