using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Web.Application.Commands.Workflows;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Workflows.Definition;

/// <summary>
/// 创建流程定义请求
/// </summary>
public record CreateDefinitionRequest(
    string Name,
    string Description,
    string Category,
    string DefinitionJson);

/// <summary>
/// 创建流程定义响应
/// </summary>
public record CreateDefinitionResponse(WorkflowDefinitionId Id, string Name);

/// <summary>
/// 创建流程定义端点
/// </summary>
public class CreateDefinitionEndpoint(IMediator mediator) : Endpoint<CreateDefinitionRequest, ResponseData<CreateDefinitionResponse>>
{
    public override void Configure()
    {
        Tags("WorkflowDefinitions");
        Description(b => b.AutoTagOverride("WorkflowDefinitions").WithSummary("创建流程定义"));
        Post("/api/admin/workflow/definitions");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.WorkflowDefinitionCreate);
    }

    public override async Task HandleAsync(CreateDefinitionRequest req, CancellationToken ct)
    {
        if (!User.TryGetUserId(out var userIdValue))
        {
            throw new KnownException("无效的用户身份", ErrorCodes.InvalidUserIdentity);
        }

        var cmd = new CreateWorkflowDefinitionCommand(
            req.Name,
            req.Description,
            req.Category,
            req.DefinitionJson ?? string.Empty,
            userIdValue);

        var id = await mediator.Send(cmd, ct);
        var response = new CreateDefinitionResponse(id, req.Name);
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}
