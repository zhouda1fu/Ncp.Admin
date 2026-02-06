using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Workflow.Instance;

/// <summary>
/// 获取所有流程实例列表端点（管理员视角）
/// </summary>
public class GetAllInstancesEndpoint(WorkflowInstanceQuery query) : Endpoint<WorkflowInstanceQueryInput, ResponseData<PagedData<WorkflowInstanceQueryDto>>>
{
    public override void Configure()
    {
        Tags("WorkflowInstances");
        Description(b => b.AutoTagOverride("WorkflowInstances"));
        Get("/api/admin/workflow/instances");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.WorkflowMonitor);
    }

    public override async Task HandleAsync(WorkflowInstanceQueryInput req, CancellationToken ct)
    {
        var result = await query.GetAllInstancesAsync(req, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
