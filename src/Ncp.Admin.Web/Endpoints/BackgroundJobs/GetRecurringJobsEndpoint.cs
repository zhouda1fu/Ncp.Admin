using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Services.BackgroundJobs;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.BackgroundJobs;

/// <summary>
/// 获取定时任务列表。
/// </summary>
/// <param name="service">应用服务。</param>
public sealed class GetRecurringJobsEndpoint(RecurringJobManagementService service)
    : EndpointWithoutRequest<ResponseData<IReadOnlyList<RecurringJobInfoDto>>>
{
    public override void Configure()
    {
        Tags("BackgroundJobs");
        Get("/api/admin/background-jobs/recurring");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.BackgroundJobView);
        Description(b => b.AutoTagOverride("BackgroundJobs").WithSummary("获取定时任务列表"));
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        await Send.OkAsync(service.GetRecurringJobs().AsResponseData(), cancellation: ct);
    }
}
