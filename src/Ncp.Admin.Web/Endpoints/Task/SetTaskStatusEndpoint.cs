using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.TaskAggregate;
using Ncp.Admin.Web.Application.Commands.ProjectTask;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ProjectTask;

/// <summary>
/// 设置任务状态请求
/// </summary>
public class SetTaskStatusRequest
{
    public Guid Id { get; set; }
    /// <summary>
    /// 0 待办 1 进行中 2 已完成 3 已取消
    /// </summary>
    public int Status { get; set; }
}

/// <summary>
/// 设置任务状态
/// </summary>
public class SetTaskStatusEndpoint(IMediator mediator) : Endpoint<SetTaskStatusRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Task");
        Put("/api/admin/tasks/{id}/status");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.TaskEdit);
    }

    public override async System.Threading.Tasks.Task HandleAsync(SetTaskStatusRequest req, CancellationToken ct)
    {
        var cmd = new SetTaskStatusCommand(new TaskId(req.Id), (Ncp.Admin.Domain.AggregatesModel.TaskAggregate.TaskStatus)req.Status);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
