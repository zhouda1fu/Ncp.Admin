using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.AnnouncementAggregate;
using Ncp.Admin.Web.Application.Commands.Announcements;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Announcements;

/// <summary>
/// 更新公告请求
/// </summary>
/// <param name="Id">公告 ID</param>
/// <param name="Title">公告标题</param>
/// <param name="Content">公告正文</param>
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
        Description(b => b.AutoTagOverride("Announcement").WithSummary("更新公告（仅草稿）"));
    }

    public override async Task HandleAsync(UpdateAnnouncementRequest req, CancellationToken ct)
    {
        await mediator.Send(new UpdateAnnouncementCommand(req.Id, req.Title, req.Content), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
