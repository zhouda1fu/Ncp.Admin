using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ProjectType;

/// <summary>
/// 获取项目类型列表
/// </summary>
public class GetProjectTypeListEndpoint(ProjectTypeQuery query) : EndpointWithoutRequest<ResponseData<IReadOnlyList<ProjectTypeDto>>>
{
    public override void Configure()
    {
        Tags("ProjectType");
        Get("/api/admin/project-types");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProjectTypeView);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await query.GetListAsync(ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
