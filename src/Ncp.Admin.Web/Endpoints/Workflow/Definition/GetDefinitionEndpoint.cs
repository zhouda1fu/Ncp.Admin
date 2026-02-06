using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.WorkflowDefinitionAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Workflow.Definition;

/// <summary>
/// 获取流程定义详情请求
/// </summary>
public record GetDefinitionRequest(WorkflowDefinitionId Id);

/// <summary>
/// 获取流程定义详情端点
/// </summary>
public class GetDefinitionEndpoint(WorkflowDefinitionQuery query) : Endpoint<GetDefinitionRequest, ResponseData<WorkflowDefinitionQueryDto>>
{
    public override void Configure()
    {
        Tags("WorkflowDefinitions");
        Description(b => b.AutoTagOverride("WorkflowDefinitions"));
        Get("/api/admin/workflow/definitions/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.WorkflowDefinitionView);
    }

    public override async Task HandleAsync(GetDefinitionRequest req, CancellationToken ct)
    {
        var result = await query.GetDefinitionByIdAsync(req.Id, ct);
        if (result == null)
            await Send.NotFoundAsync(ct);
        else
            await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
