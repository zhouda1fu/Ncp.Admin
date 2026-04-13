using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProjectIndustryAggregate;
using Ncp.Admin.Web.Application.Commands.ProjectIndustries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ProjectsIndustries;

/// <summary>
/// 删除项目行业
/// </summary>
public class DeleteProjectIndustryEndpoint(IMediator mediator) : EndpointWithoutRequest<ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("ProjectIndustry");
        Description(b => b.AutoTagOverride("ProjectIndustry").WithSummary("删除项目行业"));
        Delete("/api/admin/project-industries/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProjectIndustryEdit);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<Guid>("id");
        await mediator.Send(new DeleteProjectIndustryCommand(new ProjectIndustryId(id)), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
