using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Web.Application.Commands.Workflows;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Workflows.Instance;

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
        Description(b => b.AutoTagOverride("WorkflowInstances").WithSummary("恢复流程"));
        Post("/api/admin/workflow/instances/{id}/resume");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.WorkflowMonitor);
    }

    public override async Task HandleAsync(ResumeWorkflowRequest req, CancellationToken ct)
    {
        if (!User.TryGetUserId(out var userIdValue))
        {
            throw new KnownException("无效的用户身份", ErrorCodes.InvalidUserIdentity);
        }

        await mediator.Send(new ResumeWorkflowCommand(req.Id, userIdValue), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
