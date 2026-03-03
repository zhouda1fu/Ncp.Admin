using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProjectTypeAggregate;
using Ncp.Admin.Web.Application.Commands.ProjectType;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ProjectType;

/// <summary>
/// 创建项目类型请求
/// </summary>
public class CreateProjectTypeRequest
{
    public string Name { get; set; } = "";
    public int SortOrder { get; set; }
}

/// <summary>
/// 创建项目类型响应
/// </summary>
public record CreateProjectTypeResponse(ProjectTypeId Id);

/// <summary>
/// 创建项目类型
/// </summary>
public class CreateProjectTypeEndpoint(IMediator mediator) : Endpoint<CreateProjectTypeRequest, ResponseData<CreateProjectTypeResponse>>
{
    public override void Configure()
    {
        Tags("ProjectType");
        Post("/api/admin/project-types");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProjectTypeCreate);
    }

    public override async Task HandleAsync(CreateProjectTypeRequest req, CancellationToken ct)
    {
        var cmd = new CreateProjectTypeCommand(req.Name, req.SortOrder);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateProjectTypeResponse(id).AsResponseData(), cancellation: ct);
    }
}
