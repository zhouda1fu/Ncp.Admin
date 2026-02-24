using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.AnnouncementAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Announcement;

/// <summary>
/// 公告详情请求
/// </summary>
public record GetAnnouncementRequest(AnnouncementId Id);


/// <summary>
/// 按 ID 获取公告详情，支持按当前用户填充已读状态
/// </summary>
public class GetAnnouncementEndpoint(AnnouncementQuery query) : Endpoint<GetAnnouncementRequest, ResponseData<AnnouncementQueryDto>>
{
    public override void Configure()
    {
        Tags("Announcement");
        Get("/api/admin/announcements/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.AnnouncementView);
    }

    public override async Task HandleAsync(GetAnnouncementRequest req, CancellationToken ct)
    {
        UserId? readerId = null;
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userIdStr) && long.TryParse(userIdStr, out var uid))
            readerId = new UserId(uid);
        var result = await query.GetByIdAsync(req.Id, readerId, ct);
        if (result == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
