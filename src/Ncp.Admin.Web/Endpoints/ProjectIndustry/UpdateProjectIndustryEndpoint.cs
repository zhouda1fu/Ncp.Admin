using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProjectIndustryAggregate;
using Ncp.Admin.Web.Application.Commands.ProjectIndustry;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ProjectIndustry;

/// <summary>
/// 更新项目行业请求
/// </summary>
public class UpdateProjectIndustryRequest
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public int SortOrder { get; set; }
}

/// <summary>
/// 更新项目行业
/// </summary>
public class UpdateProjectIndustryEndpoint(IMediator mediator) : Endpoint<UpdateProjectIndustryRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("ProjectIndustry");
        Put("/api/admin/project-industries/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProjectIndustryEdit);
    }

    public override async Task HandleAsync(UpdateProjectIndustryRequest req, CancellationToken ct)
    {
        var cmd = new UpdateProjectIndustryCommand(new ProjectIndustryId(req.Id), req.Name, req.SortOrder);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
