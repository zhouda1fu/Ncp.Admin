using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Workflows.TaskEndpoints;

/// <summary>
/// 获取我的已办任务端点
/// </summary>
public class GetMyCompletedTasksEndpoint(WorkflowInstanceQuery query) : Endpoint<CompletedTaskQueryInput, ResponseData<PagedData<MyCompletedTaskQueryDto>>>
{
    public override void Configure()
    {
        Tags("WorkflowTasks");
        Description(b => b.AutoTagOverride("WorkflowTasks").WithSummary("获取我的已办任务"));
        Get("/api/admin/workflow/tasks/completed");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.WorkflowInstanceView);
    }

    public override async System.Threading.Tasks.Task HandleAsync(CompletedTaskQueryInput req, CancellationToken ct)
    {
        if (!User.TryGetUserId(out var userIdValue))
        {
            throw new KnownException("无效的用户身份", ErrorCodes.InvalidUserIdentity);
        }

        var result = await query.GetMyCompletedTasksAsync(userIdValue, req, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
