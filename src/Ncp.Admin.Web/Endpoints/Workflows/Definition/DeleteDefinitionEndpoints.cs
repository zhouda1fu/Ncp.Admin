using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Web.Application.Commands.Workflows;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Workflows.Definition;

/// <summary>
/// 删除草稿流程定义请求
/// </summary>
/// <param name="Id">流程定义 ID</param>
public record DeleteDraftDefinitionRequest(WorkflowDefinitionId Id);

/// <summary>
/// 删除草稿流程定义端点
/// </summary>
public class DeleteDraftDefinitionEndpoint(IMediator mediator)
    : Endpoint<DeleteDraftDefinitionRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("WorkflowDefinitions");
        Description(b => b.AutoTagOverride("WorkflowDefinitions").WithSummary("删除草稿流程定义"));
        Delete("/api/admin/workflow/definitions/{id}/draft");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.WorkflowDefinitionDelete);
    }

    public override async Task HandleAsync(DeleteDraftDefinitionRequest req, CancellationToken ct)
    {
        await mediator.Send(new DeleteDraftWorkflowDefinitionCommand(req.Id), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 删除已发布或已归档流程定义请求
/// </summary>
/// <param name="Id">流程定义 ID</param>
public record DeletePublishedDefinitionRequest(WorkflowDefinitionId Id);

/// <summary>
/// 删除已发布或已归档流程定义端点
/// </summary>
public class DeletePublishedDefinitionEndpoint(IMediator mediator)
    : Endpoint<DeletePublishedDefinitionRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("WorkflowDefinitions");
        Description(b => b.AutoTagOverride("WorkflowDefinitions").WithSummary("删除已发布或已归档流程定义"));
        Delete("/api/admin/workflow/definitions/{id}/published");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.WorkflowDefinitionDeletePublished);
    }

    public override async Task HandleAsync(DeletePublishedDefinitionRequest req, CancellationToken ct)
    {
        await mediator.Send(new DeletePublishedWorkflowDefinitionCommand(req.Id), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
