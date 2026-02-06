using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Workflow.Definition;

/// <summary>
/// 获取流程定义列表端点
/// </summary>
public class GetDefinitionsEndpoint(WorkflowDefinitionQuery query) : Endpoint<WorkflowDefinitionQueryInput, ResponseData<PagedData<WorkflowDefinitionQueryDto>>>
{
    public override void Configure()
    {
        Tags("WorkflowDefinitions");
        Description(b => b.AutoTagOverride("WorkflowDefinitions"));
        Get("/api/admin/workflow/definitions");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.WorkflowDefinitionView);
    }

    public override async Task HandleAsync(WorkflowDefinitionQueryInput req, CancellationToken ct)
    {
        var result = await query.GetAllDefinitionsAsync(req, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
