using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;
using NetCorePal.Extensions.Dto;

namespace Ncp.Admin.Web.Endpoints.SystemLogs;

/// <summary>
/// 获取系统日志筛选选项。
/// </summary>
/// <param name="query">查询服务。</param>
public sealed class GetSystemLogOptionsEndpoint(SystemLogQuery query)
    : EndpointWithoutRequest<ResponseData<SystemLogOptionsDto>>
{
    public override void Configure()
    {
        Tags("SystemLog");
        Description(b => b.AutoTagOverride("SystemLog").WithSummary("获取系统日志筛选选项"));
        Get("/api/admin/system-logs/options");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.SystemLogView);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await query.GetOptionsAsync(ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
