using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Web.Application.Commands.Workflow;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Workflow.TaskEndpoints;

/// <summary>
/// 驳回请求
/// </summary>
public record RejectTaskRequest(
    WorkflowInstanceId WorkflowInstanceId,
    WorkflowTaskId TaskId,
    string Comment);

/// <summary>
/// 驳回端点
/// </summary>
public class RejectTaskEndpoint(IMediator mediator) : Endpoint<RejectTaskRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("WorkflowTasks");
        Description(b => b.AutoTagOverride("WorkflowTasks"));
        Post("/api/admin/workflow/tasks/{taskId}/reject");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.WorkflowTaskApprove);
    }

    public override async System.Threading.Tasks.Task HandleAsync(RejectTaskRequest req, CancellationToken ct)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !long.TryParse(userIdString, out var userIdValue))
        {
            throw new KnownException("无效的用户身份", ErrorCodes.InvalidUserIdentity);
        }

        var cmd = new RejectTaskCommand(
            req.WorkflowInstanceId,
            req.TaskId,
            new UserId(userIdValue),
            req.Comment);

        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
