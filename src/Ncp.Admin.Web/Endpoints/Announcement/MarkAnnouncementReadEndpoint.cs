using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.AnnouncementAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Announcements;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Announcement;

public record MarkAnnouncementReadRequest(AnnouncementId Id);

/// <summary>
/// 标记当前用户对指定公告已读
/// </summary>
public class MarkAnnouncementReadEndpoint(IMediator mediator) : Endpoint<MarkAnnouncementReadRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Announcement");
        Post("/api/admin/announcements/{id}/read");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.AnnouncementView);
    }

    public override async Task HandleAsync(MarkAnnouncementReadRequest req, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out var userIdValue))
            throw new KnownException("无效的用户身份", ErrorCodes.InvalidUserIdentity);
        await mediator.Send(new MarkAnnouncementReadCommand(req.Id, new UserId(userIdValue)), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
