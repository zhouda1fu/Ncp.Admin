using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProjectStatusOptionAggregate;
using Ncp.Admin.Web.Application.Commands.ProjectStatusOptions;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ProjectsStatusOptions;

/// <summary>
/// 删除项目状态选项
/// </summary>
public class DeleteProjectStatusOptionEndpoint(IMediator mediator) : EndpointWithoutRequest<ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("ProjectStatusOption");
        Description(b => b.AutoTagOverride("ProjectStatusOption").WithSummary("删除项目状态选项"));
        Delete("/api/admin/project-status-options/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProjectStatusOptionEdit);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        await mediator.Send(new DeleteProjectStatusOptionCommand(new ProjectStatusOptionId(id)), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
