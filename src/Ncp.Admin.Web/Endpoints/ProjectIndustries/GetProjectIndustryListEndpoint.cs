using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ProjectsIndustries;

/// <summary>
/// 获取项目行业列表
/// </summary>
public class GetProjectIndustryListEndpoint(ProjectIndustryQuery query) : EndpointWithoutRequest<ResponseData<IReadOnlyList<ProjectIndustryDto>>>
{
    public override void Configure()
    {
        Tags("ProjectIndustry");
        Description(b => b.AutoTagOverride("ProjectIndustry").WithSummary("获取项目行业列表"));
        Get("/api/admin/project-industries");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProjectIndustryView);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await query.GetListAsync(ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
