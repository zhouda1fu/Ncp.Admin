using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.Application.Services.Workflow;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Workflows.TaskEndpoints;

/// <summary>
/// 获取任务退回字段请求。
/// </summary>
public record GetTaskReturnFieldsRequest(
    WorkflowInstanceId WorkflowInstanceId,
    WorkflowTaskId TaskId);

/// <summary>
/// 获取任务退回字段端点。
/// </summary>
public class GetTaskReturnFieldsEndpoint(IMediator mediator)
    : Endpoint<GetTaskReturnFieldsRequest, ResponseData<WorkflowReturnOptionsDto>>
{
    public override void Configure()
    {
        Tags("WorkflowTasks");
        Description(b => b.AutoTagOverride("WorkflowTasks").WithSummary("获取任务退回字段"));
        Get("/api/admin/workflow/tasks/{taskId}/return-fields");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.WorkflowTaskApprove);
    }

    public override async System.Threading.Tasks.Task HandleAsync(GetTaskReturnFieldsRequest req, CancellationToken ct)
    {
        if (!User.TryGetUserId(out var userIdValue))
        {
            throw new KnownException("无效的用户身份", ErrorCodes.InvalidUserIdentity);
        }

        var options = await mediator.Send(
            new GetWorkflowTaskReturnFieldsQuery(req.WorkflowInstanceId, req.TaskId, userIdValue),
            ct);
        await Send.OkAsync(options.AsResponseData(), cancellation: ct);
    }
}
