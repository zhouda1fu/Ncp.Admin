using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Project;

/// <summary>
/// 获取项目详情请求
/// </summary>
/// <param name="Id">项目 ID</param>
public record GetProjectRequest(ProjectId Id);

/// <summary>
/// 获取项目详情（含联系人、跟进记录）
/// </summary>
public class GetProjectEndpoint(ProjectQuery query)
    : Endpoint<GetProjectRequest, ResponseData<ProjectDetailDto>>
{
    public override void Configure()
    {
        Tags("Project");
        Get("/api/admin/projects/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProjectView);
    }

    public override async Task HandleAsync(GetProjectRequest req, CancellationToken ct)
    {
        var result = await query.GetDetailByIdAsync(req.Id, ct);
        if (result == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
