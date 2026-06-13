using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Services.BackgroundJobs;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.BackgroundJobs;

/// <summary>
/// 保存内置定时任务请求。
/// </summary>
/// <param name="Id">记录 ID。</param>
/// <param name="Cron">Cron 表达式。</param>
public sealed record UpsertKnownRecurringJobRequest(string Id, string Cron);

/// <summary>
/// 启用或更新系统内置定时任务。
/// </summary>
/// <param name="service">应用服务。</param>
public sealed class UpsertKnownRecurringJobEndpoint(RecurringJobManagementService service)
    : Endpoint<UpsertKnownRecurringJobRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("BackgroundJobs");
        Put("/api/admin/background-jobs/recurring/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.BackgroundJobManagement);
        Description(b => b.AutoTagOverride("BackgroundJobs").WithSummary("启用或更新系统内置定时任务"));
        Idempotency();
    }

    public override async Task HandleAsync(UpsertKnownRecurringJobRequest req, CancellationToken ct)
    {
        await Send.OkAsync(service.UpsertKnownJob(req.Id, req.Cron).AsResponseData(), cancellation: ct);
    }
}
