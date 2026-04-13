using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectIndustryAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectStatusOptionAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectTypeAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Projects;

/// <summary>
/// 获取项目列表请求
/// </summary>
/// <param name="PageIndex">页码</param>
/// <param name="PageSize">每页条数</param>
/// <param name="Name">名称关键字</param>
/// <param name="Status">状态（0 进行中 1 已归档）</param>
/// <param name="CustomerId">客户 ID</param>
/// <param name="ProjectTypeId">项目类型 ID</param>
/// <param name="ProjectStatusOptionId">项目状态选项 ID</param>
/// <param name="ProjectIndustryId">项目行业 ID</param>
public record GetProjectListRequest(
    int PageIndex = 1,
    int PageSize = 20,
    string? Name = null,
    int? Status = null,
    CustomerId? CustomerId = null,
    ProjectTypeId? ProjectTypeId = null,
    ProjectStatusOptionId? ProjectStatusOptionId = null,
    ProjectIndustryId? ProjectIndustryId = null);

/// <summary>
/// 获取项目分页列表
/// </summary>
public class GetProjectListEndpoint(ProjectQuery query)
    : Endpoint<GetProjectListRequest, ResponseData<PagedData<ProjectQueryDto>>>
{
    public override void Configure()
    {
        Tags("Project");
        Description(b => b.AutoTagOverride("Project").WithSummary("获取项目分页列表"));
        Get("/api/admin/projects");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProjectView);
    }

    public override async Task HandleAsync(GetProjectListRequest req, CancellationToken ct)
    {
        var input = new ProjectQueryInput
        {
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            Name = req.Name,
            Status = req.Status,
            CustomerId = req.CustomerId,
            ProjectTypeId = req.ProjectTypeId,
            ProjectStatusOptionId = req.ProjectStatusOptionId,
            ProjectIndustryId = req.ProjectIndustryId,
        };
        var result = await query.GetPagedAsync(input, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
