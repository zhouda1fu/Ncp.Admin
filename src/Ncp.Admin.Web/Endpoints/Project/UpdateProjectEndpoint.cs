using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectIndustryAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectStatusOptionAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectTypeAggregate;
using Ncp.Admin.Domain.AggregatesModel.RegionAggregate;
using Ncp.Admin.Web.Application.Commands.Project;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Project;

/// <summary>
/// 更新项目请求（冗余名称由前端传入）
/// </summary>
public record UpdateProjectRequest(
    ProjectId Id,
    string Name,
    ProjectTypeId ProjectTypeId,
    string ProjectTypeName,
    ProjectStatusOptionId ProjectStatusOptionId,
    string ProjectStatusOptionName,
    string? ProjectNumber,
    ProjectIndustryId ProjectIndustryId,
    string ProjectIndustryName,
    RegionId ProvinceRegionId,
    string ProvinceName,
    RegionId CityRegionId,
    string CityName,
    RegionId DistrictRegionId,
    string DistrictName,
    DateOnly? StartDate,
    decimal? Budget,
    decimal? PurchaseAmount,
    string? ProjectContent);

/// <summary>
/// 更新项目
/// </summary>
public class UpdateProjectEndpoint(IMediator mediator) : Endpoint<UpdateProjectRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Project");
        Put("/api/admin/projects/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProjectEdit);
    }

    public override async Task HandleAsync(UpdateProjectRequest req, CancellationToken ct)
    {
        var cmd = new UpdateProjectCommand(
            req.Id,
            req.Name,
            req.ProjectTypeId,
            req.ProjectTypeName ?? "",
            req.ProjectStatusOptionId,
            req.ProjectStatusOptionName ?? "",
            req.ProjectNumber ?? string.Empty,
            req.ProjectIndustryId,
            req.ProjectIndustryName ?? "",
            req.ProvinceRegionId,
            req.ProvinceName ?? "",
            req.CityRegionId,
            req.CityName ?? "",
            req.DistrictRegionId,
            req.DistrictName ?? "",
            req.StartDate,
            req.Budget ?? 0,
            req.PurchaseAmount ?? 0,
            req.ProjectContent ?? string.Empty);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
