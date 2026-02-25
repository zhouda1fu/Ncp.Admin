using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.TaskAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.ProjectTask;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ProjectTask;

/// <summary>
/// 更新任务请求
/// </summary>
public class UpdateTaskRequest
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string? Description { get; set; }
    public long? AssigneeId { get; set; }
    public string? DueDate { get; set; }
}

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
            new TaskId(req.Id),
            req.Title,
            req.Description,
            req.AssigneeId.HasValue ? new UserId(req.AssigneeId.Value) : null,
            dueDate);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
