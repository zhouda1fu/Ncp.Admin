using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Commands.Dashboard;
using Ncp.Admin.Web.Services;

namespace Ncp.Admin.Web.Endpoints.Dashboard;

/// <summary>
/// 保存指定日期的行事历便签（内容为空则删除）。
/// </summary>
/// <param name="Date">便签日期。</param>
/// <param name="Content">便签正文。</param>
public record SaveUserCalendarMemoRequest(DateOnly Date, string Content);

/// <summary>
/// 保存行事历便签（所有登录用户可用）。
/// </summary>
public class SaveUserCalendarMemoEndpoint(IMediator mediator)
    : Endpoint<SaveUserCalendarMemoRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Dashboard");
        Description(b => b.AutoTagOverride("Dashboard").WithSummary("保存行事历便签"));
        Put("/api/admin/dashboard/calendar-memo");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(SaveUserCalendarMemoRequest req, CancellationToken ct)
    {
        if (!User.TryGetUserId(out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        await mediator.Send(new SaveUserCalendarMemoCommand(userId, req.Date, req.Content), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
