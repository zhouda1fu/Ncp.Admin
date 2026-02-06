using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Web.Application.Commands.Workflow;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Workflow.Definition;

/// <summary>
/// 删除流程定义请求
/// </summary>
public record DeleteDefinitionRequest(WorkflowDefinitionId Id);

/// <summary>
/// 删除流程定义端点
/// </summary>
public class DeleteDefinitionEndpoint(IMediator mediator) : Endpoint<DeleteDefinitionRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("WorkflowDefinitions");
        Description(b => b.AutoTagOverride("WorkflowDefinitions"));
        Delete("/api/admin/workflow/definitions/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.WorkflowDefinitionDelete);
    }

    public override async Task HandleAsync(DeleteDefinitionRequest req, CancellationToken ct)
    {
        var cmd = new DeleteWorkflowDefinitionCommand(req.Id);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
