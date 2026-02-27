using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Web.Application.Commands.Workflow;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Workflow.Instance;

/// <summary>
/// 发起流程请求
/// </summary>
public record StartWorkflowRequest(
    WorkflowDefinitionId WorkflowDefinitionId,
    string BusinessKey,
    string BusinessType,
    string Title,
    string Variables,
    string Remark);

/// <summary>
/// 发起流程响应
/// </summary>
public record StartWorkflowResponse(WorkflowInstanceId Id, string Title);

/// <summary>
/// 发起流程端点
/// </summary>
public class StartWorkflowEndpoint(IMediator mediator) : Endpoint<StartWorkflowRequest, ResponseData<StartWorkflowResponse>>
{
    public override void Configure()
    {
        Tags("WorkflowInstances");
        Description(b => b.AutoTagOverride("WorkflowInstances"));
        Post("/api/admin/workflow/instances");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.WorkflowStart);
    }

    public override async Task HandleAsync(StartWorkflowRequest req, CancellationToken ct)
    {
        var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdString) || !long.TryParse(userIdString, out var userIdValue))
        {
            throw new KnownException("无效的用户身份", ErrorCodes.InvalidUserIdentity);
        }

        var userName = User.FindFirstValue(ClaimTypes.Name);

        var cmd = new StartWorkflowCommand(
            req.WorkflowDefinitionId,
            req.BusinessKey,
            req.BusinessType,
            req.Title,
            new UserId(userIdValue),
            userName ?? string.Empty,
            req.Variables,
            req.Remark);

        var id = await mediator.Send(cmd, ct);
        var response = new StartWorkflowResponse(id, req.Title);
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}
