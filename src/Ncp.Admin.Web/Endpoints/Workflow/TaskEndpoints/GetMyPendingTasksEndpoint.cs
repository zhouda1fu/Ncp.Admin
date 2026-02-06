using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Endpoints.Workflow.TaskEndpoints;

/// <summary>
/// 获取我的待办任务端点
/// </summary>
public class GetMyPendingTasksEndpoint(WorkflowInstanceQuery query) : Endpoint<PendingTaskQueryInput, ResponseData<PagedData<MyPendingTaskQueryDto>>>
{
    public override void Configure()
    {
        Tags("WorkflowTasks");
        Description(b => b.AutoTagOverride("WorkflowTasks"));
        Get("/api/admin/workflow/tasks/pending");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async System.Threading.Tasks.Task HandleAsync(PendingTaskQueryInput req, CancellationToken ct)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !long.TryParse(userIdString, out var userIdValue))
        {
            throw new KnownException("无效的用户身份", ErrorCodes.InvalidUserIdentity);
        }

        var result = await query.GetMyPendingTasksAsync(new UserId(userIdValue), req, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
