using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Domain.AggregatesModel.TaskAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ProjectTask;

/// <summary>
/// 获取任务列表请求
/// </summary>
/// <param name="PageIndex">页码</param>
/// <param name="PageSize">每页条数</param>
/// <param name="ProjectId">项目 ID 筛选</param>
/// <param name="Status">状态筛选</param>
public record GetTaskListRequest(int PageIndex = 1, int PageSize = 20, ProjectId? ProjectId = null, int? Status = null);

/// <summary>
/// 获取任务分页列表
/// </summary>
public class GetTaskListEndpoint(TaskQuery query)
    : Endpoint<GetTaskListRequest, ResponseData<PagedData<TaskQueryDto>>>
{
    public override void Configure()
    {
        Tags("Task");
        Get("/api/admin/tasks");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.TaskView);
    }

    public override async System.Threading.Tasks.Task HandleAsync(GetTaskListRequest req, CancellationToken ct)
    {
        var input = new TaskQueryInput
        {
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            ProjectId = req.ProjectId,
            Status = req.Status.HasValue ? (ProjectTaskStatus?)req.Status.Value : null,
        };
        var result = await query.GetPagedAsync(input, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
