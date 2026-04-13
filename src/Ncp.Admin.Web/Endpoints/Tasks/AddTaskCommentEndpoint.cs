using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.TaskAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Tasks;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ProjectsTask;

/// <summary>
/// 添加任务评论请求
/// </summary>
/// <param name="TaskId">任务 ID</param>
/// <param name="Content">评论内容</param>
public record AddTaskCommentRequest(ProjectTaskId TaskId, string Content);

/// <summary>
/// 添加任务评论（当前用户为评论人）
/// </summary>
public class AddTaskCommentEndpoint(IMediator mediator) : Endpoint<AddTaskCommentRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Task");
        Description(b => b.AutoTagOverride("Task").WithSummary("添加任务评论（当前用户为评论人）"));
        Post("/api/admin/tasks/{taskId}/comments");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.TaskEdit);
    }

    public override async System.Threading.Tasks.Task HandleAsync(AddTaskCommentRequest req, CancellationToken ct)
    {
        if (!User.TryGetUserId(out var uid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        var cmd = new AddTaskCommentCommand(req.TaskId, req.Content, uid);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
