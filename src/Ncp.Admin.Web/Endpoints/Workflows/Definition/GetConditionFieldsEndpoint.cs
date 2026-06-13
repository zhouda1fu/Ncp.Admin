using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Commands.Workflows;
using Ncp.Admin.Web.Application.Services.Workflow;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Workflows.Definition;

/// <summary>
/// 获取流程条件字段请求
/// </summary>
public record GetConditionFieldsRequest
{
    public string Category { get; set; } = string.Empty;
}

/// <summary>
/// 获取流程条件字段端点
/// GET /api/admin/workflow/condition-fields/{category}
/// </summary>
public class GetConditionFieldsEndpoint(WorkflowConditionFieldsProvider conditionFieldsProvider)
    : Endpoint<GetConditionFieldsRequest, ResponseData<List<ConditionFieldDto>>>
{
    public override void Configure()
    {
        Tags("WorkflowDefinitions");
        Description(b => b.AutoTagOverride("WorkflowDefinitions").WithSummary("获取流程条件字段"));
        Get("/api/admin/workflow/condition-fields/{category}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.WorkflowDefinitionView);
    }

    public override async Task HandleAsync(GetConditionFieldsRequest req, CancellationToken ct)
    {
        var category = req.Category ?? string.Empty;
        if (!string.Equals(category, WorkflowBusinessTypes.CreateUser, StringComparison.Ordinal))
        {
            await Send.OkAsync(new List<ConditionFieldDto>().AsResponseData(), cancellation: ct);
            return;
        }

        var fields = conditionFieldsProvider.GetFields(category);
        await Send.OkAsync(fields.AsResponseData(), cancellation: ct);
    }
}
