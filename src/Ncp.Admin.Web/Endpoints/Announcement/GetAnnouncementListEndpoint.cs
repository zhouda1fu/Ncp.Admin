using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Announcement;

/// <summary>
/// 公告列表请求（继承分页与筛选）
/// </summary>
public class GetAnnouncementListRequest : AnnouncementQueryInput { }

/// <summary>
/// 获取公告分页列表，支持按当前用户填充已读状态
/// </summary>
public class GetAnnouncementListEndpoint(AnnouncementQuery query) : Endpoint<GetAnnouncementListRequest, ResponseData<PagedData<AnnouncementQueryDto>>>
{
    public override void Configure()
    {
        Tags("Announcement");
        Get("/api/admin/announcements");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.AnnouncementView);
    }

    public override async Task HandleAsync(GetAnnouncementListRequest req, CancellationToken ct)
    {
        UserId? readerId = null;
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrEmpty(userIdStr) && long.TryParse(userIdStr, out var uid))
            readerId = new UserId(uid);
        var result = await query.GetPagedAsync(req, readerId, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
