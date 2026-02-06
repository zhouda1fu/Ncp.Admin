using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Web.Application.Commands.Workflow;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Workflow.Definition;

/// <summary>
/// 更新流程定义请求
/// </summary>
public record UpdateDefinitionRequest(
    WorkflowDefinitionId Id,
    string Name,
    string Description,
    string Category,
    string DefinitionJson,
    IEnumerable<WorkflowNodeData> Nodes);

/// <summary>
/// 更新流程定义端点
/// </summary>
public class UpdateDefinitionEndpoint(IMediator mediator) : Endpoint<UpdateDefinitionRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("WorkflowDefinitions");
        Description(b => b.AutoTagOverride("WorkflowDefinitions"));
        Put("/api/admin/workflow/definitions");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.WorkflowDefinitionEdit);
    }

    public override async Task HandleAsync(UpdateDefinitionRequest req, CancellationToken ct)
    {
        var cmd = new UpdateWorkflowDefinitionCommand(
            req.Id,
            req.Name,
            req.Description,
            req.Category,
            req.DefinitionJson,
            req.Nodes);

        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
