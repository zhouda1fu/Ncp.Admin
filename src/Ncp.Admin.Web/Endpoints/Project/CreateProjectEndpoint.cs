using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectIndustryAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectStatusOptionAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectTypeAggregate;
using Ncp.Admin.Domain.AggregatesModel.RegionAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Project;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Project;

/// <summary>
/// 创建项目请求（冗余名称由前端传入）
/// </summary>
public record CreateProjectRequest(
    string Name,
    string? Description,
    CustomerId CustomerId,
    string CustomerName,
    ProjectTypeId ProjectTypeId,
    string ProjectTypeName,
    ProjectStatusOptionId ProjectStatusOptionId,
    string ProjectStatusOptionName,
    ProjectIndustryId ProjectIndustryId,
    string ProjectIndustryName,
    RegionId ProvinceRegionId,
    string ProvinceName,
    RegionId CityRegionId,
    string CityName,
    RegionId DistrictRegionId,
    string DistrictName,
    string? ProjectNumber,
    DateOnly? StartDate,
    string? ProjectEstimate,
    decimal? PurchaseAmount,
    string? ProjectContent);

/// <summary>
/// 创建项目（当前用户为创建人）
/// </summary>
public class CreateProjectEndpoint(IMediator mediator) : Endpoint<CreateProjectRequest, ResponseData<CreateProjectResponse>>
{
    public override void Configure()
    {
        Tags("Project");
        Post("/api/admin/projects");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProjectCreate);
    }

    public override async Task HandleAsync(CreateProjectRequest req, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out var uid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        var creatorName = User.FindFirstValue(ClaimTypes.Name) ?? "";
        var cmd = new CreateProjectCommand(
            new UserId(uid),
            creatorName,
            req.CustomerId,
            req.CustomerName ?? "",
            req.ProjectTypeId,
            req.ProjectTypeName ?? "",
            req.ProjectStatusOptionId,
            req.ProjectStatusOptionName ?? "",
            req.ProjectIndustryId,
            req.ProjectIndustryName ?? "",
            req.ProvinceRegionId,
            req.ProvinceName ?? "",
            req.CityRegionId,
            req.CityName ?? "",
            req.DistrictRegionId,
            req.DistrictName ?? "",
            req.Name,
            req.Description ?? string.Empty,
            req.ProjectNumber ?? string.Empty,
            req.StartDate,
            req.ProjectEstimate ?? string.Empty,
            req.PurchaseAmount ?? 0,
            req.ProjectContent ?? string.Empty);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateProjectResponse(id).AsResponseData(), cancellation: ct);
    }
}

public record CreateProjectResponse(ProjectId Id);
