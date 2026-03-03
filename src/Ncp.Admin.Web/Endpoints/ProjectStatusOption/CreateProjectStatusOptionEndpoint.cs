using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProjectStatusOptionAggregate;
using Ncp.Admin.Web.Application.Commands.ProjectStatusOption;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ProjectStatusOption;

/// <summary>
/// 创建项目状态选项请求
/// </summary>
public class CreateProjectStatusOptionRequest
{
    public string Name { get; set; } = "";
    public string? Code { get; set; }
    public int SortOrder { get; set; }
}

/// <summary>
/// 创建项目状态选项响应
/// </summary>
public record CreateProjectStatusOptionResponse(ProjectStatusOptionId Id);

/// <summary>
/// 创建项目状态选项
/// </summary>
public class CreateProjectStatusOptionEndpoint(IMediator mediator) : Endpoint<CreateProjectStatusOptionRequest, ResponseData<CreateProjectStatusOptionResponse>>
{
    public override void Configure()
    {
        Tags("ProjectStatusOption");
        Post("/api/admin/project-status-options");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProjectStatusOptionCreate);
    }

    public override async Task HandleAsync(CreateProjectStatusOptionRequest req, CancellationToken ct)
    {
        var cmd = new CreateProjectStatusOptionCommand(req.Name, req.Code ?? string.Empty, req.SortOrder);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateProjectStatusOptionResponse(id).AsResponseData(), cancellation: ct);
    }
}
