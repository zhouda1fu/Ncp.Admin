using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Web.Application.Commands.Project;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Project;

/// <summary>
/// 更新项目请求
/// </summary>
public class UpdateProjectRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Description { get; set; }
}

/// <summary>
/// 更新项目
/// </summary>
public class UpdateProjectEndpoint(IMediator mediator) : Endpoint<UpdateProjectRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Project");
        Put("/api/admin/projects/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProjectEdit);
    }

    public override async Task HandleAsync(UpdateProjectRequest req, CancellationToken ct)
    {
        var id = new ProjectId(req.Id);
        var cmd = new UpdateProjectCommand(id, req.Name, req.Description);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
