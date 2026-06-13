using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;
using NetCorePal.Extensions.Dto;

namespace Ncp.Admin.Web.Endpoints.SystemLogs;

/// <summary>
/// 获取系统日志列表请求。
/// </summary>
/// <param name="PageIndex">页码。</param>
/// <param name="PageSize">每页数量。</param>
/// <param name="CountTotal">总数量。</param>
/// <param name="Level">级别。</param>
/// <param name="Category">分类。</param>
/// <param name="Keyword">关键字。</param>
/// <param name="TraceId">追踪 ID。</param>
/// <param name="HasException">是否Has Exception。</param>
/// <param name="StartTime">开始时间。</param>
/// <param name="EndTime">结束时间。</param>
public sealed record GetSystemLogListRequest(
    int PageIndex = 1,
    int PageSize = 20,
    bool CountTotal = true,
    string? Level = null,
    string? Category = null,
    string? Keyword = null,
    string? TraceId = null,
    bool? HasException = null,
    DateTimeOffset? StartTime = null,
    DateTimeOffset? EndTime = null);

/// <summary>
/// 获取系统日志分页列表。
/// </summary>
/// <param name="query">查询服务。</param>
public sealed class GetSystemLogListEndpoint(SystemLogQuery query)
    : Endpoint<GetSystemLogListRequest, ResponseData<PagedData<SystemLogListItemDto>>>
{
    public override void Configure()
    {
        Tags("SystemLog");
        Description(b => b.AutoTagOverride("SystemLog").WithSummary("获取系统日志分页列表"));
        Get("/api/admin/system-logs");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.SystemLogView);
    }

    public override async Task HandleAsync(GetSystemLogListRequest req, CancellationToken ct)
    {
        var result = await query.GetPagedAsync(new SystemLogQueryInput
        {
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            CountTotal = req.CountTotal,
            Level = req.Level,
            Category = req.Category,
            Keyword = req.Keyword,
            TraceId = req.TraceId,
            HasException = req.HasException,
            StartTime = req.StartTime,
            EndTime = req.EndTime
        }, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
