using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;
using NetCorePal.Extensions.Dto;

namespace Ncp.Admin.Web.Endpoints.SystemLogs;

/// <summary>
/// 获取系统日志详情请求。
/// </summary>
/// <param name="Id">记录 ID。</param>
public sealed record GetSystemLogDetailRequest(long Id);

/// <summary>
/// 获取系统日志详情。
/// </summary>
/// <param name="query">查询服务。</param>
public sealed class GetSystemLogDetailEndpoint(SystemLogQuery query)
    : Endpoint<GetSystemLogDetailRequest, ResponseData<SystemLogDetailDto>>
{
    public override void Configure()
    {
        Tags("SystemLog");
        Description(b => b.AutoTagOverride("SystemLog").WithSummary("获取系统日志详情"));
        Get("/api/admin/system-logs/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.SystemLogView);
    }

    public override async Task HandleAsync(GetSystemLogDetailRequest req, CancellationToken ct)
    {
        var result = await query.GetDetailAsync(req.Id, ct);
        if (result is null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
