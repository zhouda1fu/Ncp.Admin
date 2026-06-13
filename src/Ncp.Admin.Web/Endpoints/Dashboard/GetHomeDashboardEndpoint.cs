using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.Extensions;
using Ncp.Admin.Web.Services;

namespace Ncp.Admin.Web.Endpoints.Dashboard;

/// <summary>
/// 获取当前用户后台首页工作台汇总（工作流待办、未读通知、行事历、卡片排序）。
/// </summary>
/// <param name="CalendarYear">行事历年份；省略则用当前月。</param>
/// <param name="CalendarMonth">行事历月份 1–12；省略则用当前月。</param>
public record GetHomeDashboardRequest(int? CalendarYear = null, int? CalendarMonth = null);

/// <summary>
/// 获取首页工作台数据；按 JWT 权限码过滤可分配卡片的数据，置顶区与行事历对所有登录用户返回。
/// </summary>
public class GetHomeDashboardEndpoint(HomeDashboardQuery query)
    : Endpoint<GetHomeDashboardRequest, ResponseData<HomeDashboardDto>>
{
    public override void Configure()
    {
        Tags("Dashboard");
        Description(b => b.AutoTagOverride("Dashboard").WithSummary("获取后台首页工作台汇总"));
        Get("/api/admin/dashboard/home");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetHomeDashboardRequest req, CancellationToken ct)
    {
        if (!User.TryGetUserId(out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var granted = User.GetAppPermissionCodes();
        var result = await query.GetForCurrentUserAsync(
            userId,
            granted,
            req.CalendarYear,
            req.CalendarMonth,
            ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
