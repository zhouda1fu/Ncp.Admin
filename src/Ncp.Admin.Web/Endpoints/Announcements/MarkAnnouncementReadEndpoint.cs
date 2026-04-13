using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.AnnouncementAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Announcements;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Announcements;

/// <summary>
/// 标记公告已读请求
/// </summary>
/// <param name="Id">公告 ID</param>
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
        Description(b => b.AutoTagOverride("Announcement").WithSummary("标记当前用户对公告已读"));
    }

    public override async Task HandleAsync(MarkAnnouncementReadRequest req, CancellationToken ct)
    {
        if (!User.TryGetUserId(out var userId))
            throw new KnownException("无效的用户身份", ErrorCodes.InvalidUserIdentity);
        await mediator.Send(new MarkAnnouncementReadCommand(req.Id, userId), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
