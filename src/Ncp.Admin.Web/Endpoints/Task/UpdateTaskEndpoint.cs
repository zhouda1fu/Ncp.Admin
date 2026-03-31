using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.TaskAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.TaskModule;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ProjectTask;

/// <summary>
/// 更新任务请求
/// </summary>
/// <param name="Id">任务 ID</param>
/// <param name="Title">标题</param>
/// <param name="Description">描述</param>
/// <param name="AssigneeId">负责人用户 ID</param>
/// <param name="DueDate">截止日期（yyyy-MM-dd）</param>
public record UpdateTaskRequest(
    ProjectTaskId Id,
    string Title,
    string? Description,
    UserId? AssigneeId,
    string? DueDate);

/// <summary>
/// 更新任务
/// </summary>
public class UpdateTaskEndpoint(IMediator mediator) : Endpoint<UpdateTaskRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Task");
        Put("/api/admin/tasks/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.TaskEdit);
    }

    public override async System.Threading.Tasks.Task HandleAsync(UpdateTaskRequest req, CancellationToken ct)
    {
        DateOnly? dueDate = null;
        if (!string.IsNullOrWhiteSpace(req.DueDate) && DateOnly.TryParse(req.DueDate, out var d))
            dueDate = d;
        var cmd = new UpdateTaskCommand(
            req.Id,
            req.Title,
            req.Description,
            req.AssigneeId,
            dueDate);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
