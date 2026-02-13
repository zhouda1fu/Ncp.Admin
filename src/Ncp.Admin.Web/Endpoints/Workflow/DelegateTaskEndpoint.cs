using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Web.Application.Commands.Workflow;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Workflow;

/// <summary>
/// 委托审批任务请求模型
/// </summary>
public record DelegateTaskRequest(
    WorkflowInstanceId InstanceId,
    WorkflowTaskId TaskId,
    UserId DelegateToUserId,
    string DelegateToUserName,
    string Comment);

/// <summary>
/// 委托审批任务
/// </summary>
public class DelegateTaskEndpoint(IMediator mediator) : Endpoint<DelegateTaskRequest, ResponseData<WorkflowTaskId>>
{
    public override void Configure()
    {
        Tags("WorkflowTasks");
        Description(b => b.AutoTagOverride("WorkflowTasks"));
        Post("/api/admin/workflow/delegate");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.WorkflowTaskApprove);
    }

    public override async Task HandleAsync(DelegateTaskRequest req, CancellationToken ct)
    {
        var command = new DelegateWorkflowTaskCommand(
            req.InstanceId, 
            req.TaskId, 
            req.DelegateToUserId, 
            req.DelegateToUserName, 
            req.Comment);

        var result = await mediator.Send(command, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
