using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Web.Application.Commands.Projects;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Project;

public record AddProjectContactRequest(
    ProjectId ProjectId,
    CustomerContactId? CustomerContactId,
    string Name,
    string Position,
    string Mobile,
    string OfficePhone,
    string QQ,
    string Wechat,
    string Email,
    bool IsPrimary,
    string Remark);

public record AddProjectContactResponse(ProjectContactId Id);

public class AddProjectContactEndpoint(IMediator mediator) : Endpoint<AddProjectContactRequest, ResponseData<AddProjectContactResponse>>
{
    public override void Configure()
    {
        Tags("Project");
        Post("/api/admin/projects/{projectId}/contacts");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProjectEdit);
    }

    public override async Task HandleAsync(AddProjectContactRequest req, CancellationToken ct)
    {
        var cmd = new AddProjectContactCommand(
            req.ProjectId, req.CustomerContactId, req.Name ?? string.Empty, req.Position ?? string.Empty,
            req.Mobile ?? string.Empty, req.OfficePhone ?? string.Empty, req.QQ ?? string.Empty,
            req.Wechat ?? string.Empty, req.Email ?? string.Empty, req.IsPrimary, req.Remark ?? string.Empty);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new AddProjectContactResponse(id).AsResponseData(), cancellation: ct);
    }
}
