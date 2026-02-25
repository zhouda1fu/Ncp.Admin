using System.Security.Claims;
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
/// 添加任务评论请求
/// </summary>
public class AddTaskCommentRequest
{
    public Guid TaskId { get; set; }
    public string Content { get; set; } = "";
}

/// <summary>
/// 添加任务评论（当前用户为评论人）
/// </summary>
public class AddTaskCommentEndpoint(IMediator mediator) : Endpoint<AddTaskCommentRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Task");
        Post("/api/admin/tasks/{taskId}/comments");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.TaskEdit);
    }

    public override async System.Threading.Tasks.Task HandleAsync(AddTaskCommentRequest req, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out var uid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        var cmd = new AddTaskCommentCommand(new TaskId(req.TaskId), req.Content, new UserId(uid));
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
