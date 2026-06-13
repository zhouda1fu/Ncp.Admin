using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Services.BackgroundJobs;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.BackgroundJobs;

/// <summary>
/// 移除定时任务请求。
/// </summary>
/// <param name="Id">记录 ID。</param>
public sealed record RemoveRecurringJobRequest(string Id);

/// <summary>
/// 移除定时任务。
/// </summary>
/// <param name="service">应用服务。</param>
public sealed class RemoveRecurringJobEndpoint(RecurringJobManagementService service)
    : Endpoint<RemoveRecurringJobRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("BackgroundJobs");
        Delete("/api/admin/background-jobs/recurring/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.BackgroundJobManagement);
        Description(b => b.AutoTagOverride("BackgroundJobs").WithSummary("移除定时任务"));
        Idempotency();
    }

    public override async Task HandleAsync(RemoveRecurringJobRequest req, CancellationToken ct)
    {
        await Send.OkAsync(service.Remove(req.Id).AsResponseData(), cancellation: ct);
    }
}
