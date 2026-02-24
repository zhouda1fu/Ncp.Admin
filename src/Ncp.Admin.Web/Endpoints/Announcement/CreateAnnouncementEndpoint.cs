using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.AnnouncementAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Announcements;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Announcement;

public record CreateAnnouncementRequest(string Title, string Content);

public record CreateAnnouncementResponse(AnnouncementId Id);

/// <summary>
/// 创建公告（草稿），发布人为当前登录用户
/// </summary>
public class CreateAnnouncementEndpoint(IMediator mediator, UserQuery userQuery) : Endpoint<CreateAnnouncementRequest, ResponseData<CreateAnnouncementResponse>>
{
    public override void Configure()
    {
        Tags("Announcement");
        Post("/api/admin/announcements");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.AnnouncementCreate);
    }

    public override async Task HandleAsync(CreateAnnouncementRequest req, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out var userIdValue))
            throw new KnownException("无效的用户身份", ErrorCodes.InvalidUserIdentity);
        var user = await userQuery.GetUserByIdAsync(new UserId(userIdValue), ct)
            ?? throw new KnownException("未找到当前用户", ErrorCodes.UserNotFound);
        var cmd = new CreateAnnouncementCommand(
            new UserId(userIdValue),
            user.RealName ?? user.Name,
            req.Title,
            req.Content);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateAnnouncementResponse(id).AsResponseData(), cancellation: ct);
    }
}
