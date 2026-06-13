using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.Services;

namespace Ncp.Admin.Web.Endpoints.Dashboard;

/// <summary>
/// 获取指定日期的行事历便签。
/// </summary>
/// <param name="Date">便签日期（yyyy-MM-dd）。</param>
public record GetUserCalendarMemoRequest(DateOnly Date);

/// <summary>
/// 获取行事历便签（所有登录用户可用）。
/// </summary>
public class GetUserCalendarMemoEndpoint(UserCalendarMemoQuery query)
    : Endpoint<GetUserCalendarMemoRequest, ResponseData<UserCalendarMemoDto?>>
{
    public override void Configure()
    {
        Tags("Dashboard");
        Description(b => b.AutoTagOverride("Dashboard").WithSummary("获取行事历便签"));
        Get("/api/admin/dashboard/calendar-memo");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(GetUserCalendarMemoRequest req, CancellationToken ct)
    {
        if (!User.TryGetUserId(out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var memo = await query.GetByDateAsync(userId, req.Date, ct);
        await Send.OkAsync(memo.AsResponseData(), cancellation: ct);
    }
}
