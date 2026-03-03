using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ProjectStatusOption;

/// <summary>
/// 获取项目状态选项列表
/// </summary>
public class GetProjectStatusOptionListEndpoint(ProjectStatusOptionQuery query) : EndpointWithoutRequest<ResponseData<IReadOnlyList<ProjectStatusOptionDto>>>
{
    public override void Configure()
    {
        Tags("ProjectStatusOption");
        Get("/api/admin/project-status-options");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProjectStatusOptionView);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await query.GetListAsync(ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
