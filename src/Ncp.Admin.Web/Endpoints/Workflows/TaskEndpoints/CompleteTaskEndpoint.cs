using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Web.Application.Commands.Workflows;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Workflows.TaskEndpoints;

/// <summary>
/// 完成通知任务请求。
/// </summary>
public record CompleteTaskRequest(
    WorkflowInstanceId WorkflowInstanceId,
    WorkflowTaskId TaskId,
    string? Comment);

/// <summary>
/// 完成通知任务端点。
/// </summary>
public class CompleteTaskEndpoint(IMediator mediator) : Endpoint<CompleteTaskRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("WorkflowTasks");
        Description(b => b.AutoTagOverride("WorkflowTasks").WithSummary("完成通知任务"));
        Post("/api/admin/workflow/tasks/{taskId}/complete");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.WorkflowTaskApprove);
    }

    public override async Task HandleAsync(CompleteTaskRequest req, CancellationToken ct)
    {
        if (!User.TryGetUserId(out var userIdValue))
        {
            throw new KnownException("无效的用户身份", ErrorCodes.InvalidUserIdentity);
        }

        try
        {
            await mediator.Send(
                new CompleteWorkflowNotificationTaskCommand(
                    req.WorkflowInstanceId,
                    req.TaskId,
                    userIdValue,
                    req.Comment ?? string.Empty),
                ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new KnownException("该任务已被处理，请刷新后重试", ErrorCodes.WorkflowTaskConcurrencyConflict);
        }

        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
