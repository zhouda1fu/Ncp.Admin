using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProjectStatusOptionAggregate;
using Ncp.Admin.Web.Application.Commands.ProjectStatusOption;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ProjectStatusOption;

/// <summary>
/// 更新项目状态选项请求
/// </summary>
public class UpdateProjectStatusOptionRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string? Code { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>
/// 更新项目状态选项
/// </summary>
public class UpdateProjectStatusOptionEndpoint(IMediator mediator) : Endpoint<UpdateProjectStatusOptionRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("ProjectStatusOption");
        Put("/api/admin/project-status-options/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProjectStatusOptionEdit);
    }

    public override async Task HandleAsync(UpdateProjectStatusOptionRequest req, CancellationToken ct)
    {
        var cmd = new UpdateProjectStatusOptionCommand(new ProjectStatusOptionId(req.Id), req.Name, req.Code ?? string.Empty, req.SortOrder);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
