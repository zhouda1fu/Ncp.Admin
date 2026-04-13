using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.TaskAggregate;
using Ncp.Admin.Web.Application.Commands.Tasks;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ProjectsTask;

/// <summary>
/// 删除任务
/// </summary>
public class DeleteTaskEndpoint(IMediator mediator) : EndpointWithoutRequest<ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Task");
        Description(b => b.AutoTagOverride("Task").WithSummary("删除任务"));
        Delete("/api/admin/tasks/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.TaskEdit);
    }

    public override async global::System.Threading.Tasks.Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        await mediator.Send(new DeleteTaskCommand(new ProjectTaskId(id)), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
