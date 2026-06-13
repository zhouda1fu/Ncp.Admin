using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Services.BackgroundJobs;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.BackgroundJobs;

/// <summary>
/// 获取系统内置定时任务。
/// </summary>
/// <param name="service">应用服务。</param>
public sealed class GetKnownRecurringJobsEndpoint(RecurringJobManagementService service)
    : EndpointWithoutRequest<ResponseData<IReadOnlyList<KnownRecurringJobDto>>>
{
    public override void Configure()
    {
        Tags("BackgroundJobs");
        Get("/api/admin/background-jobs/recurring/known");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.BackgroundJobView);
        Description(b => b.AutoTagOverride("BackgroundJobs").WithSummary("获取系统内置定时任务"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.OkAsync(service.GetKnownJobs().AsResponseData(), cancellation: ct);
    }
}
