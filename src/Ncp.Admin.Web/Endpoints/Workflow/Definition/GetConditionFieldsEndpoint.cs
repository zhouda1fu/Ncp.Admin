using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Commands.Workflow;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.Application.Services.Workflow;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Workflow.Definition;

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
public class GetConditionFieldsEndpoint(ProductCategoryQuery productCategoryQuery)
    : Endpoint<GetConditionFieldsRequest, ResponseData<List<ConditionFieldDto>>>
{
    public override void Configure()
    {
        Tags("WorkflowDefinitions");
        Description(b => b.AutoTagOverride("WorkflowDefinitions"));
        Get("/api/admin/workflow/condition-fields/{category}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.WorkflowDefinitionView);
    }

    public override async Task HandleAsync(GetConditionFieldsRequest req, CancellationToken ct)
    {
        var category = req.Category ?? string.Empty;
        var fields = WorkflowConditionFieldsProvider.GetFields(category);

        if (string.Equals(category, WorkflowBusinessTypes.Order, StringComparison.Ordinal))
        {
            var trees = await productCategoryQuery.GetTreeAsync(includeInvisible: true, cancellationToken: ct);
            foreach (var c in Flatten(trees).Where(x => x.IsDiscount))
            {
                fields.Add(new ConditionFieldDto(
                    $"CategoryDiscountPoints.{c.Id}",
                    $"优惠点数-{c.Name}",
                    "number"));
            }
        }

        await Send.OkAsync(fields.AsResponseData(), cancellation: ct);
    }

    private static IEnumerable<ProductCategoryTreeDto> Flatten(IEnumerable<ProductCategoryTreeDto> nodes)
    {
        foreach (var n in nodes)
        {
            yield return n;
            if (n.Children != null)
            {
                foreach (var c in Flatten(n.Children))
                    yield return c;
            }
        }
    }
}
