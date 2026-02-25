using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Project;

/// <summary>
/// 获取项目列表请求
/// </summary>
public class GetProjectListRequest : ProjectQueryInput { }

/// <summary>
/// 获取项目分页列表
/// </summary>
public class GetProjectListEndpoint(ProjectQuery query)
    : Endpoint<GetProjectListRequest, ResponseData<PagedData<ProjectQueryDto>>>
{
    public override void Configure()
    {
        Tags("Project");
        Get("/api/admin/projects");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProjectView);
    }

    public override async Task HandleAsync(GetProjectListRequest req, CancellationToken ct)
    {
        var result = await query.GetPagedAsync(req, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
