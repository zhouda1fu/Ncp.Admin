using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProjectTypeAggregate;
using Ncp.Admin.Web.Application.Commands.ProjectType;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ProjectType;

/// <summary>
/// 更新项目类型请求
/// </summary>
public class UpdateProjectTypeRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public int SortOrder { get; set; }
}

/// <summary>
/// 更新项目类型
/// </summary>
public class UpdateProjectTypeEndpoint(IMediator mediator) : Endpoint<UpdateProjectTypeRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("ProjectType");
        Put("/api/admin/project-types/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProjectTypeEdit);
    }

    public override async Task HandleAsync(UpdateProjectTypeRequest req, CancellationToken ct)
    {
        var cmd = new UpdateProjectTypeCommand(new ProjectTypeId(req.Id), req.Name, req.SortOrder);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
