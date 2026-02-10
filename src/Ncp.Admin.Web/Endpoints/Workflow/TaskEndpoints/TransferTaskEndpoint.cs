using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Web.Application.Commands.Workflow;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Workflow.TaskEndpoints;

/// <summary>
/// 转办请求
/// </summary>
public record TransferTaskRequest(
    WorkflowInstanceId WorkflowInstanceId,
    WorkflowTaskId TaskId,
    UserId NewAssigneeId,
    string NewAssigneeName,
    string Comment);

/// <summary>
/// 转办端点
/// </summary>
public class TransferTaskEndpoint(IMediator mediator) : Endpoint<TransferTaskRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("WorkflowTasks");
        Description(b => b.AutoTagOverride("WorkflowTasks"));
        Post("/api/admin/workflow/tasks/{taskId}/transfer");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.WorkflowTaskApprove);
    }

    public override async System.Threading.Tasks.Task HandleAsync(TransferTaskRequest req, CancellationToken ct)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !long.TryParse(userIdString, out var userIdValue))
        {
            throw new KnownException("无效的用户身份", ErrorCodes.InvalidUserIdentity);
        }

        var cmd = new TransferTaskCommand(
            req.WorkflowInstanceId,
            req.TaskId,
            new UserId(userIdValue),
            req.NewAssigneeId,
            req.NewAssigneeName,
            req.Comment);

        try
        {
            await mediator.Send(cmd, ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new KnownException("该任务已被处理，请刷新后重试", ErrorCodes.WorkflowTaskConcurrencyConflict);
        }

        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
