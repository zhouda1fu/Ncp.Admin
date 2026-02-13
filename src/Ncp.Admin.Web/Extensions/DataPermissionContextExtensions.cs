using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ncp.Admin.Web.Context;
using NetCorePal.Context;

namespace Ncp.Admin.Web.Extensions;

/// <summary>
/// 数据权限上下文扩展，按 NetCorePal 自定义上下文规范注册 Carrier/Source 处理器。
/// 参见 <see href="https://netcorepal.github.io/netcorepal-cloud-framework/zh/context/custom-context-type/"/>
/// </summary>
public static class DataPermissionContextExtensions
{
    /// <summary>
    /// 添加 DataPermissionContext 的上下文传递支持（HttpClient、CAP 等场景）。
    /// 需在 AddContext() 之后调用。
    /// </summary>
    public static IServiceCollection AddDataPermissionContext(this IServiceCollection services)
    {
        services.TryAddSingleton<IContextCarrierHandler, DataPermissionContextCarrierHandler>();
        services.TryAddSingleton<IContextSourceHandler, DataPermissionContextSourceHandler>();
        return services;
    }
}
