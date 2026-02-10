using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Web.Application.Commands.Workflow;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Workflow.Instance;

/// <summary>
/// 挂起流程请求
/// </summary>
public record SuspendWorkflowRequest(WorkflowInstanceId Id);

/// <summary>
/// 挂起流程端点
/// </summary>
public class SuspendWorkflowEndpoint(IMediator mediator) : Endpoint<SuspendWorkflowRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("WorkflowInstances");
        Description(b => b.AutoTagOverride("WorkflowInstances"));
        Post("/api/admin/workflow/instances/{id}/suspend");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.WorkflowMonitor);
    }

    public override async Task HandleAsync(SuspendWorkflowRequest req, CancellationToken ct)
    {
        await mediator.Send(new SuspendWorkflowCommand(req.Id), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
