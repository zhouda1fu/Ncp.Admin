using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.AnnouncementAggregate;
using Ncp.Admin.Web.Application.Commands.Announcements;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Announcement;

public record PublishAnnouncementRequest(AnnouncementId Id);

/// <summary>
/// 发布公告（仅草稿可发布）
/// </summary>
public class PublishAnnouncementEndpoint(IMediator mediator) : Endpoint<PublishAnnouncementRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Announcement");
        Post("/api/admin/announcements/{id}/publish");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.AnnouncementPublish);
    }

    public override async Task HandleAsync(PublishAnnouncementRequest req, CancellationToken ct)
    {
        await mediator.Send(new PublishAnnouncementCommand(req.Id), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
