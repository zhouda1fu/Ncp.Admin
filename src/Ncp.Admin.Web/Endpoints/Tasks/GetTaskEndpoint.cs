using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.TaskAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ProjectsTask;

/// <summary>
/// 获取任务详情请求
/// </summary>
/// <param name="Id">任务 ID</param>
public record GetTaskRequest(ProjectTaskId Id);

/// <summary>
/// 获取任务详情（含评论）
/// </summary>
public class GetTaskEndpoint(TaskQuery query)
    : Endpoint<GetTaskRequest, ResponseData<TaskQueryDto>>
{
    public override void Configure()
    {
        Tags("Task");
        Description(b => b.AutoTagOverride("Task").WithSummary("获取任务详情（含评论）"));
        Get("/api/admin/tasks/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.TaskView);
    }

    public override async global::System.Threading.Tasks.Task HandleAsync(GetTaskRequest req, CancellationToken ct)
    {
        var result = await query.GetByIdAsync(req.Id, ct);
        if (result == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
