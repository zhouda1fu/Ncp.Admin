using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Web.Application.Commands.Workflow;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Workflow.Instance;

/// <summary>
/// 撤销流程请求
/// </summary>
public record CancelWorkflowRequest(WorkflowInstanceId Id);

/// <summary>
/// 撤销流程端点
/// </summary>
public class CancelWorkflowEndpoint(IMediator mediator) : Endpoint<CancelWorkflowRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("WorkflowInstances");
        Description(b => b.AutoTagOverride("WorkflowInstances"));
        Post("/api/admin/workflow/instances/{id}/cancel");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.WorkflowCancel);
    }

    public override async Task HandleAsync(CancelWorkflowRequest req, CancellationToken ct)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !long.TryParse(userIdString, out var userIdValue))
        {
            throw new KnownException("无效的用户身份", ErrorCodes.InvalidUserIdentity);
        }

        var cmd = new CancelWorkflowCommand(req.Id, new UserId(userIdValue));
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
