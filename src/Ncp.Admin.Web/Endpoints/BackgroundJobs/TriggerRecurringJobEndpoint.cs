using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Services.BackgroundJobs;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.BackgroundJobs;

/// <summary>
/// 立即执行定时任务（任务 ID 来自路由，无需请求体）。
/// </summary>
public sealed class TriggerRecurringJobEndpoint(RecurringJobManagementService service)
    : EndpointWithoutRequest<ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("BackgroundJobs");
        Post("/api/admin/background-jobs/recurring/{id}/trigger");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.BackgroundJobTrigger);
        Description(b => b.AutoTagOverride("BackgroundJobs").WithSummary("立即执行定时任务"));
        Idempotency();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<string>("id") ?? string.Empty;
        await Send.OkAsync(service.Trigger(id).AsResponseData(), cancellation: ct);
    }
}
