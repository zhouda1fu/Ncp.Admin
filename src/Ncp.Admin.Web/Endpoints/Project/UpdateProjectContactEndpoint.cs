using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Web.Application.Commands.Projects;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Project;

/// <summary>
/// 更新项目联系人请求
/// </summary>
public record UpdateProjectContactRequest(
    ProjectId ProjectId,
    ProjectContactId ContactId,
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

/// <summary>
/// 更新项目联系人
/// </summary>
public class UpdateProjectContactEndpoint(IMediator mediator) : Endpoint<UpdateProjectContactRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Project");
        Put("/api/admin/projects/{projectId}/contacts/{contactId}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ProjectEdit);
    }

    public override async Task HandleAsync(UpdateProjectContactRequest req, CancellationToken ct)
    {
        var cmd = new UpdateProjectContactCommand(
            req.ProjectId, req.ContactId, req.CustomerContactId, req.Name ?? string.Empty, req.Position ?? string.Empty,
            req.Mobile ?? string.Empty, req.OfficePhone ?? string.Empty, req.QQ ?? string.Empty,
            req.Wechat ?? string.Empty, req.Email ?? string.Empty, req.IsPrimary, req.Remark ?? string.Empty);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
