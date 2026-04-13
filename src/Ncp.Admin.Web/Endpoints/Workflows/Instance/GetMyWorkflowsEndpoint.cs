using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Workflows.Instance;

/// <summary>
/// 获取我发起的流程端点
/// </summary>
public class GetMyWorkflowsEndpoint(WorkflowInstanceQuery query) : Endpoint<WorkflowInstanceQueryInput, ResponseData<PagedData<WorkflowInstanceQueryDto>>>
{
    public override void Configure()
    {
        Tags("WorkflowInstances");
        Description(b => b.AutoTagOverride("WorkflowInstances").WithSummary("获取我发起的流程"));
        Get("/api/admin/workflow/my-workflows");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.WorkflowInstanceView);
    }

    public override async Task HandleAsync(WorkflowInstanceQueryInput req, CancellationToken ct)
    {
        if (!User.TryGetUserId(out var userIdValue))
        {
            throw new KnownException("无效的用户身份", ErrorCodes.InvalidUserIdentity);
        }

        var result = await query.GetMyInitiatedWorkflowsAsync(userIdValue, req, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
