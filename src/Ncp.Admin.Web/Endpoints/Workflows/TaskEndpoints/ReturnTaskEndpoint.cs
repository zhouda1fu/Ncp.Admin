using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Web.Application.Commands.Workflows;
using Ncp.Admin.Web.Application.Services.Workflow;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Workflows.TaskEndpoints;

/// <summary>
/// 退回任务请求。
/// </summary>
public record ReturnTaskRequest(
    WorkflowInstanceId WorkflowInstanceId,
    WorkflowTaskId TaskId,
    string Comment,
    IReadOnlyList<WorkflowReturnFieldDto> ReturnFields);

/// <summary>
/// 退回任务端点。
/// </summary>
public class ReturnTaskEndpoint(IMediator mediator) : Endpoint<ReturnTaskRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("WorkflowTasks");
        Description(b => b.AutoTagOverride("WorkflowTasks").WithSummary("退回任务"));
        Post("/api/admin/workflow/tasks/{taskId}/return");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.WorkflowTaskApprove);
    }

    public override async System.Threading.Tasks.Task HandleAsync(ReturnTaskRequest req, CancellationToken ct)
    {
        if (!User.TryGetUserId(out var userIdValue))
        {
            throw new KnownException("无效的用户身份", ErrorCodes.InvalidUserIdentity);
        }

        var cmd = new ReturnTaskCommand(
            req.WorkflowInstanceId,
            req.TaskId,
            userIdValue,
            req.Comment,
            req.ReturnFields);

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
