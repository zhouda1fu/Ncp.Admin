using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Domain.AggregatesModel.TaskAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.TaskModule;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ProjectTask;

/// <summary>
/// 创建任务请求
/// </summary>
/// <param name="ProjectId">项目 ID</param>
/// <param name="Title">标题</param>
/// <param name="Description">描述</param>
/// <param name="AssigneeId">负责人用户 ID</param>
/// <param name="DueDate">截止日期（yyyy-MM-dd）</param>
/// <param name="SortOrder">排序</param>
public record CreateTaskRequest(
    ProjectId ProjectId,
    string Title,
    string? Description,
    UserId? AssigneeId,
    string? DueDate,
    int SortOrder);

/// <summary>
/// 创建任务
/// </summary>
public class CreateTaskEndpoint(IMediator mediator) : Endpoint<CreateTaskRequest, ResponseData<CreateTaskResponse>>
{
    public override void Configure()
    {
        Tags("Task");
        Post("/api/admin/tasks");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.TaskCreate);
    }

    public override async System.Threading.Tasks.Task HandleAsync(CreateTaskRequest req, CancellationToken ct)
    {
        DateOnly? dueDate = null;
        if (!string.IsNullOrWhiteSpace(req.DueDate) && DateOnly.TryParse(req.DueDate, out var d))
            dueDate = d;
        var cmd = new CreateTaskCommand(
            req.ProjectId,
            req.Title,
            req.Description,
            req.AssigneeId,
            dueDate,
            req.SortOrder);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateTaskResponse(id).AsResponseData(), cancellation: ct);
    }
}

public record CreateTaskResponse(ProjectTaskId Id);
