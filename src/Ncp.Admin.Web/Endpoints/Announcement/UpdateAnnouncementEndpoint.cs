using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.AnnouncementAggregate;
using Ncp.Admin.Web.Application.Commands.Announcements;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Announcement;

public record UpdateAnnouncementRequest(AnnouncementId Id, string Title, string Content);

/// <summary>
/// 更新公告（仅草稿可更新）
/// </summary>
public class UpdateAnnouncementEndpoint(IMediator mediator) : Endpoint<UpdateAnnouncementRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Announcement");
        Put("/api/admin/announcements/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.AnnouncementEdit);
    }

    public override async Task HandleAsync(UpdateAnnouncementRequest req, CancellationToken ct)
    {
        await mediator.Send(new UpdateAnnouncementCommand(req.Id, req.Title, req.Content), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
