using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Web.Application.Commands.Workflow;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Workflow.Definition;

/// <summary>
/// 发布流程定义请求
/// </summary>
public record PublishDefinitionRequest(WorkflowDefinitionId Id);

/// <summary>
/// 发布流程定义端点
/// </summary>
public class PublishDefinitionEndpoint(IMediator mediator) : Endpoint<PublishDefinitionRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("WorkflowDefinitions");
        Description(b => b.AutoTagOverride("WorkflowDefinitions"));
        Post("/api/admin/workflow/definitions/{id}/publish");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.WorkflowDefinitionPublish);
    }

    public override async Task HandleAsync(PublishDefinitionRequest req, CancellationToken ct)
    {
        var cmd = new PublishWorkflowDefinitionCommand(req.Id);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
