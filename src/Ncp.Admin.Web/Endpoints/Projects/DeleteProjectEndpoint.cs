using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Web.Application.Commands.Projects;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Projects;

/// <summary>
/// 删除项目
/// </summary>
public class DeleteProjectEndpoint(IMediator mediator) : EndpointWithoutRequest<ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Project");
        Description(b => b.AutoTagOverride("Project").WithSummary("删除项目"));
        Delete("/api/admin/projects/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProjectEdit);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        await mediator.Send(new DeleteProjectCommand(new ProjectId(id)), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
