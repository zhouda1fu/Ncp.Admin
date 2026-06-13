using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Commands.Dashboard;
using Ncp.Admin.Web.Extensions;
using Ncp.Admin.Web.Services;

namespace Ncp.Admin.Web.Endpoints.Dashboard;

/// <summary>
/// 保存当前用户首页可拖拽卡片排序。
/// </summary>
/// <param name="CardOrder">卡片 key 有序列表（仅保存当前有权展示的卡片）。</param>
public record SaveUserHomeDashboardLayoutRequest(IReadOnlyList<string> CardOrder);

/// <summary>
/// 保存首页卡片排序（每用户独立偏好，所有登录用户可用）。
/// </summary>
public class SaveUserHomeDashboardLayoutEndpoint(IMediator mediator)
    : Endpoint<SaveUserHomeDashboardLayoutRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Dashboard");
        Description(b => b.AutoTagOverride("Dashboard").WithSummary("保存首页卡片排序"));
        Put("/api/admin/dashboard/home-layout");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(SaveUserHomeDashboardLayoutRequest req, CancellationToken ct)
    {
        if (!User.TryGetUserId(out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        await mediator.Send(
            new SaveUserHomeDashboardLayoutCommand(userId, req.CardOrder, User.GetAppPermissionCodes()),
            ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
