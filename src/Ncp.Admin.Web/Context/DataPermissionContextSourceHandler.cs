using System.Text.Json;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure.Services;
using NetCorePal.Context;

namespace Ncp.Admin.Web.Context;

/// <summary>
/// 从入站请求（HttpClient 响应、CAP 消息等）提取 DataPermissionContext。
/// 参见 <see href="https://netcorepal.github.io/netcorepal-cloud-framework/zh/context/custom-context-type/"/>
/// </summary>
public sealed class DataPermissionContextSourceHandler : IContextSourceHandler
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    public Type ContextType => typeof(DataPermissionContext);

    public object? Extract(IContextSource source)
    {
        var json = source.Get(DataPermissionContext.ContextKey);
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            var dto = JsonSerializer.Deserialize<DataPermissionContextDto>(json, JsonOptions);
            if (dto == null)
                return null;

            var scope = (DataScope)dto.Scope;
            var userId = dto.UserId.HasValue ? new UserId(dto.UserId.Value) : null;
            var deptId = dto.DeptId.HasValue ? new DeptId(dto.DeptId.Value) : null;
            var authorizedDeptIds = (dto.AuthorizedDeptIds ?? []).Select(id => new DeptId(id)).ToList();
            return new DataPermissionContext(scope, userId, deptId, authorizedDeptIds);
        }
        catch
        {
            return null;
        }
    }

    private sealed record DataPermissionContextDto(int Scope, long? UserId, long? DeptId, List<long>? AuthorizedDeptIds);
}
