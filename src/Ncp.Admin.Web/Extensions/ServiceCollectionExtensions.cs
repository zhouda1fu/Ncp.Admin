using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Extensions;

/// <summary>
/// 服务集合扩展方法
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 自动注册所有实现 IQuery 接口的查询类
    /// 参考框架的 AddRepositories 模式
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="assembly">要扫描的程序集</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddQueries(this IServiceCollection services, Assembly assembly)
    {
        var queryTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && typeof(IQuery).IsAssignableFrom(t));

        foreach (var queryType in queryTypes)
        {
            services.AddScoped(queryType);
        }

        return services;
    }

    /// <summary>
    /// 自动注册非聚合根实体仓储（未继承 <see cref="IRepository{TEntity,TKey}"/> 的 I*Repository 接口及其实现）。
    /// </summary>
    public static IServiceCollection AddCustomEntityRepositories(this IServiceCollection services, Assembly assembly)
    {
        const string repositoryNamespace = "Ncp.Admin.Infrastructure.Repositories";
        var repositoryTypes = assembly.GetTypes()
            .Where(t => t.Namespace == repositoryNamespace)
            .ToList();

        var customRepositoryInterfaces = repositoryTypes
            .Where(t => t.IsInterface
                && t.Name.StartsWith('I')
                && t.Name.EndsWith("Repository")
                && !IsAggregateRepository(t))
            .ToList();

        foreach (var interfaceType in customRepositoryInterfaces)
        {
            var implementationName = interfaceType.Name[1..];
            var implementationType = repositoryTypes.FirstOrDefault(t =>
                t.IsClass && !t.IsAbstract && t.Name == implementationName);
            if (implementationType is not null)
                services.AddScoped(interfaceType, implementationType);
        }

        return services;
    }

    private static bool IsAggregateRepository(Type interfaceType) =>
        interfaceType.GetInterfaces().Any(i =>
            i.IsGenericType && i.GetGenericTypeDefinition().Name.StartsWith("IRepository", StringComparison.Ordinal));
}
