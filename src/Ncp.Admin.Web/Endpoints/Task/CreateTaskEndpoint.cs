using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Domain.AggregatesModel.TaskAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.ProjectTask;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ProjectTask;

/// <summary>
/// 创建任务请求
/// </summary>
public class CreateTaskRequest
{
    public Guid ProjectId { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public long? AssigneeId { get; set; }
    public string? DueDate { get; set; }
    public int SortOrder { get; set; }
}

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
            new ProjectId(req.ProjectId),
            req.Title,
            req.Description,
            req.AssigneeId.HasValue ? new UserId(req.AssigneeId.Value) : null,
            dueDate,
            req.SortOrder);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateTaskResponse(id).AsResponseData(), cancellation: ct);
    }
}

public record CreateTaskResponse(TaskId Id);
