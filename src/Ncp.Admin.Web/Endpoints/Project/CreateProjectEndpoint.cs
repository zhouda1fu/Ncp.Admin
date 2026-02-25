using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Project;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Project;

/// <summary>
/// 创建项目请求
/// </summary>
public class CreateProjectRequest
{
    public string Name { get; set; } = "";
    public string? Description { get; set; }
}

/// <summary>
/// 创建项目（当前用户为创建人）
/// </summary>
public class CreateProjectEndpoint(IMediator mediator) : Endpoint<CreateProjectRequest, ResponseData<CreateProjectResponse>>
{
    public override void Configure()
    {
        Tags("Project");
        Post("/api/admin/projects");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProjectCreate);
    }

    public override async Task HandleAsync(CreateProjectRequest req, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out var uid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        var cmd = new CreateProjectCommand(new UserId(uid), req.Name, req.Description);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateProjectResponse(id).AsResponseData(), cancellation: ct);
    }
}

public record CreateProjectResponse(ProjectId Id);
