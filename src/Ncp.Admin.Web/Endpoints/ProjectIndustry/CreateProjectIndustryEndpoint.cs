using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProjectIndustryAggregate;
using Ncp.Admin.Web.Application.Commands.ProjectIndustry;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ProjectIndustry;

/// <summary>
/// 创建项目行业请求
/// </summary>
/// <param name="Name">名称</param>
/// <param name="SortOrder">排序</param>
public record CreateProjectIndustryRequest(string Name, int SortOrder);

/// <summary>
/// 创建项目行业响应
/// </summary>
public record CreateProjectIndustryResponse(ProjectIndustryId Id);

/// <summary>
/// 创建项目行业
/// </summary>
public class CreateProjectIndustryEndpoint(IMediator mediator) : Endpoint<CreateProjectIndustryRequest, ResponseData<CreateProjectIndustryResponse>>
{
    public override void Configure()
    {
        Tags("ProjectIndustry");
        Post("/api/admin/project-industries");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProjectIndustryCreate);
    }

    public override async Task HandleAsync(CreateProjectIndustryRequest req, CancellationToken ct)
    {
        var cmd = new CreateProjectIndustryCommand(req.Name, req.SortOrder);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateProjectIndustryResponse(id).AsResponseData(), cancellation: ct);
    }
}
