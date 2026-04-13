using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Workflows.TaskEndpoints;

/// <summary>
/// 获取我的待办任务端点
/// </summary>
public class GetMyPendingTasksEndpoint(WorkflowInstanceQuery query) : Endpoint<PendingTaskQueryInput, ResponseData<PagedData<MyPendingTaskQueryDto>>>
{
    public override void Configure()
    {
        Tags("WorkflowTasks");
        Description(b => b.AutoTagOverride("WorkflowTasks").WithSummary("获取我的待办任务"));
        Get("/api/admin/workflow/tasks/pending");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.WorkflowInstanceView);
    }

    public override async Task HandleAsync(PendingTaskQueryInput req, CancellationToken ct)
    {
        if (!User.TryGetUserId(out var userIdValue))
        {
            throw new KnownException("无效的用户身份", ErrorCodes.InvalidUserIdentity);
        }

        var result = await query.GetMyPendingTasksAsync(userIdValue, req, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
