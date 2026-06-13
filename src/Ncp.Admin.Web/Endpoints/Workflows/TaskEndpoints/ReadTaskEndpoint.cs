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
/// 标记抄送任务已读请求。
/// </summary>
public record ReadTaskRequest(
    WorkflowInstanceId WorkflowInstanceId,
    WorkflowTaskId TaskId,
    string? Comment);

/// <summary>
/// 标记抄送任务已读端点。
/// </summary>
public class ReadTaskEndpoint(IMediator mediator) : Endpoint<ReadTaskRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("WorkflowTasks");
        Description(b => b.AutoTagOverride("WorkflowTasks").WithSummary("标记抄送任务已读"));
        Post("/api/admin/workflow/tasks/{taskId}/read");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.WorkflowTaskApprove);
    }

    public override async Task HandleAsync(ReadTaskRequest req, CancellationToken ct)
    {
        if (!User.TryGetUserId(out var userIdValue))
        {
            throw new KnownException("无效的用户身份", ErrorCodes.InvalidUserIdentity);
        }

        try
        {
            await mediator.Send(
                new ReadWorkflowTaskCommand(req.WorkflowInstanceId, req.TaskId, userIdValue, req.Comment ?? string.Empty),
                ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new KnownException("该任务已被处理，请刷新后重试", ErrorCodes.WorkflowTaskConcurrencyConflict);
        }

        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
