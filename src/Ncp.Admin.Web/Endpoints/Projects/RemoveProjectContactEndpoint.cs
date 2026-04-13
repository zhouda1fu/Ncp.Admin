using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Web.Application.Commands.Projects;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Projects;

/// <summary>
/// 移除项目联系人请求
/// </summary>
public record RemoveProjectContactRequest(ProjectId ProjectId, ProjectContactId ContactId);

/// <summary>
/// 移除项目联系人
/// </summary>
public class RemoveProjectContactEndpoint(IMediator mediator) : Endpoint<RemoveProjectContactRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Project");
        Description(b => b.AutoTagOverride("Project").WithSummary("移除项目联系人"));
        Delete("/api/admin/projects/{projectId}/contacts/{contactId}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProjectEdit);
    }

    public override async Task HandleAsync(RemoveProjectContactRequest req, CancellationToken ct)
    {
        await mediator.Send(new RemoveProjectContactCommand(req.ProjectId, req.ContactId), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
