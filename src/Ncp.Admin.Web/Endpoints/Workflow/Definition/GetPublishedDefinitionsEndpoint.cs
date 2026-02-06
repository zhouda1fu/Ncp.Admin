using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Workflow.Definition;

/// <summary>
/// 获取已发布的流程定义列表端点（供发起流程时选择）
/// </summary>
public class GetPublishedDefinitionsEndpoint(WorkflowDefinitionQuery query) : EndpointWithoutRequest<ResponseData<List<WorkflowDefinitionQueryDto>>>
{
    public override void Configure()
    {
        Tags("WorkflowDefinitions");
        Description(b => b.AutoTagOverride("WorkflowDefinitions"));
        Get("/api/admin/workflow/definitions/published");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.WorkflowStart);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await query.GetPublishedDefinitionsAsync(ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
