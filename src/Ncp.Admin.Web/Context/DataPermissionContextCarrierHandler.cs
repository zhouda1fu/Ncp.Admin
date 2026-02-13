using System.Text.Json;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Services;
using NetCorePal.Context;

namespace Ncp.Admin.Web.Context;

/// <summary>
/// 将 DataPermissionContext 注入到出站请求（HttpClient、CAP 等），实现跨服务传递。
/// 参见 <see href="https://netcorepal.github.io/netcorepal-cloud-framework/zh/context/custom-context-type/"/>
/// </summary>
public sealed class DataPermissionContextCarrierHandler : IContextCarrierHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public Type ContextType => typeof(DataPermissionContext);

    public void Inject(IContextCarrier carrier, object? context)
    {
        if (context is not DataPermissionContext ctx)
            return;

        var dto = new DataPermissionContextDto(
            (int)ctx.Scope,
            ctx.UserId?.Id,
            ctx.DeptId?.Id,
            ctx.AuthorizedDeptIds.Select(d => d.Id).ToList());
        var json = JsonSerializer.Serialize(dto, JsonOptions);
        carrier.Set(DataPermissionContext.ContextKey, json);
    }

    public object? Initial() => null;

    private sealed record DataPermissionContextDto(int Scope, long? UserId, long? DeptId, List<long> AuthorizedDeptIds);
}
