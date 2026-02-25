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
public class GetTaskListRequest
{
    public int PageIndex { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public Guid? ProjectId { get; set; }
    public int? Status { get; set; }
}

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
            ProjectId = req.ProjectId.HasValue ? new ProjectId(req.ProjectId.Value) : null,
            Status = req.Status.HasValue ? (Ncp.Admin.Domain.AggregatesModel.TaskAggregate.TaskStatus?)req.Status.Value : null,
        };
        var result = await query.GetPagedAsync(input, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
