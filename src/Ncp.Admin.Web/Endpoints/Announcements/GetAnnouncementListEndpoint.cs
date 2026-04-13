using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.AnnouncementAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Announcements;

/// <summary>
/// 公告列表请求（分页与筛选）
/// </summary>
/// <param name="PageIndex">页码</param>
/// <param name="PageSize">每页条数</param>
/// <param name="Title">标题关键字</param>
/// <param name="Status">状态</param>
/// <param name="PublisherId">发布人</param>
public record GetAnnouncementListRequest(
    int PageIndex = 1,
    int PageSize = 20,
    string? Title = null,
    AnnouncementStatus? Status = null,
    UserId? PublisherId = null);

/// <summary>
/// 获取公告分页列表，支持按当前用户填充已读状态
/// </summary>
public class GetAnnouncementListEndpoint(AnnouncementQuery query) : Endpoint<GetAnnouncementListRequest, ResponseData<PagedData<AnnouncementQueryDto>>>
{
    public override void Configure()
    {
        Tags("Announcement");
        Description(b => b.AutoTagOverride("Announcement").WithSummary("获取公告分页列表，支持按当前用户填充已读状态"));
        Get("/api/admin/announcements");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.AnnouncementView);
    }

    public override async Task HandleAsync(GetAnnouncementListRequest req, CancellationToken ct)
    {
        UserId? readerId = User.GetUserIdOrNull();
        var input = new AnnouncementQueryInput
        {
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            Title = req.Title,
            Status = req.Status,
            PublisherId = req.PublisherId,
        };
        var result = await query.GetPagedAsync(input, readerId, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
