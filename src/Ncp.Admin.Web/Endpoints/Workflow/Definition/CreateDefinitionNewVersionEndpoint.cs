using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Web.Application.Commands.Workflow;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Workflow.Definition;

/// <summary>
/// 基于已有流程定义创建新版本请求
/// </summary>
/// <param name="Id">源流程定义ID</param>
public record CreateDefinitionNewVersionRequest(WorkflowDefinitionId Id);

/// <summary>
/// 基于已有流程定义创建新版本响应
/// </summary>
/// <param name="Id">新流程定义ID</param>
/// <param name="Name">流程名称</param>
/// <param name="Version">新版本号</param>
public record CreateDefinitionNewVersionResponse(
    WorkflowDefinitionId Id,
    string Name,
    int Version);

/// <summary>
/// 基于已有流程定义创建新版本端点
/// </summary>
public class CreateDefinitionNewVersionEndpoint(IMediator mediator)
    : Endpoint<CreateDefinitionNewVersionRequest, ResponseData<CreateDefinitionNewVersionResponse>>
{
    public override void Configure()
    {
        Tags("WorkflowDefinitions");
        Description(b => b.AutoTagOverride("WorkflowDefinitions"));
        Post("/api/admin/workflow/definitions/{id}/new-version");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.WorkflowDefinitionCreate);
    }

    public override async Task HandleAsync(CreateDefinitionNewVersionRequest req, CancellationToken ct)
    {
        var cmd = new CreateWorkflowDefinitionNewVersionCommand(req.Id);

        var newId = await mediator.Send(cmd, ct);

        // 目前领域创建新版本时会继承原有名称与版本+1，响应中仅返回 ID/名称/版本号
        var response = new CreateDefinitionNewVersionResponse(newId, string.Empty, 0);

        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}

