using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Web.Application.Commands.Workflow;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Workflow.Instance;

/// <summary>
/// 恢复流程请求
/// </summary>
public record ResumeWorkflowRequest(WorkflowInstanceId Id);

/// <summary>
/// 恢复流程端点
/// </summary>
public class ResumeWorkflowEndpoint(IMediator mediator) : Endpoint<ResumeWorkflowRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("WorkflowInstances");
        Description(b => b.AutoTagOverride("WorkflowInstances"));
        Post("/api/admin/workflow/instances/{id}/resume");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.WorkflowMonitor);
    }

    public override async Task HandleAsync(ResumeWorkflowRequest req, CancellationToken ct)
    {
        await mediator.Send(new ResumeWorkflowCommand(req.Id), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
