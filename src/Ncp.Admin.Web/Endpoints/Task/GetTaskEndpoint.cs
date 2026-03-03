using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.TaskAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ProjectTask;

/// <summary>
/// 获取任务详情请求
/// </summary>
public record GetTaskRequest(Guid Id);

/// <summary>
/// 获取任务详情（含评论）
/// </summary>
public class GetTaskEndpoint(TaskQuery query)
    : Endpoint<GetTaskRequest, ResponseData<TaskQueryDto>>
{
    public override void Configure()
    {
        Tags("Task");
        Get("/api/admin/tasks/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.TaskView);
    }

    public override async global::System.Threading.Tasks.Task HandleAsync(GetTaskRequest req, CancellationToken ct)
    {
        var result = await query.GetByIdAsync(new ProjectTaskId(req.Id), ct);
        if (result == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
