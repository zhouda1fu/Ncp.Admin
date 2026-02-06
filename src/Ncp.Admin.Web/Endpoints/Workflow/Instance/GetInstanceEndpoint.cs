using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Workflow.Instance;

/// <summary>
/// 获取流程实例详情请求
/// </summary>
public record GetInstanceRequest(WorkflowInstanceId Id);

/// <summary>
/// 获取流程实例详情端点（包含审批时间线）
/// </summary>
public class GetInstanceEndpoint(WorkflowInstanceQuery query) : Endpoint<GetInstanceRequest, ResponseData<WorkflowInstanceDetailQueryDto>>
{
    public override void Configure()
    {
        Tags("WorkflowInstances");
        Description(b => b.AutoTagOverride("WorkflowInstances"));
        Get("/api/admin/workflow/instances/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.WorkflowInstanceView);
    }

    public override async Task HandleAsync(GetInstanceRequest req, CancellationToken ct)
    {
        var result = await query.GetInstanceDetailAsync(req.Id, ct);
        if (result == null)
            await Send.NotFoundAsync(ct);
        else
            await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
