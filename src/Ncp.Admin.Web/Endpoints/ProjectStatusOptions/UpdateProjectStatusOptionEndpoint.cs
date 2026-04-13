using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProjectStatusOptionAggregate;
using Ncp.Admin.Web.Application.Commands.ProjectStatusOptions;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ProjectsStatusOptions;

/// <summary>
/// 更新项目状态选项请求
/// </summary>
/// <param name="Id">项目状态选项 ID</param>
/// <param name="Name">名称</param>
/// <param name="Code">编码</param>
/// <param name="SortOrder">排序</param>
public record UpdateProjectStatusOptionRequest(ProjectStatusOptionId Id, string Name, string? Code, int SortOrder);

/// <summary>
/// 更新项目状态选项
/// </summary>
public class UpdateProjectStatusOptionEndpoint(IMediator mediator) : Endpoint<UpdateProjectStatusOptionRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("ProjectStatusOption");
        Description(b => b.AutoTagOverride("ProjectStatusOption").WithSummary("更新项目状态选项"));
        Put("/api/admin/project-status-options/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProjectStatusOptionEdit);
    }

    public override async Task HandleAsync(UpdateProjectStatusOptionRequest req, CancellationToken ct)
    {
        var cmd = new UpdateProjectStatusOptionCommand(req.Id, req.Name, req.Code ?? string.Empty, req.SortOrder);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
