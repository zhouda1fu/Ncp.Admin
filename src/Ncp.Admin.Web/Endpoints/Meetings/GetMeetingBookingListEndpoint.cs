using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.MeetingAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Meetings;

/// <summary>
/// 会议室预订列表请求（分页与筛选）
/// </summary>
/// <param name="PageIndex">页码</param>
/// <param name="PageSize">每页条数</param>
/// <param name="MeetingRoomId">会议室 ID 筛选</param>
/// <param name="BookerId">预订人 ID 筛选</param>
/// <param name="StartFrom">开始时间起</param>
/// <param name="StartTo">开始时间止</param>
public record GetMeetingBookingListRequest(
    int PageIndex = 1,
    int PageSize = 20,
    MeetingRoomId? MeetingRoomId = null,
    UserId? BookerId = null,
    DateTimeOffset? StartFrom = null,
    DateTimeOffset? StartTo = null);

/// <summary>
/// 获取会议室预订分页列表
/// </summary>
public class GetMeetingBookingListEndpoint(MeetingBookingQuery query)
    : Endpoint<GetMeetingBookingListRequest, ResponseData<PagedData<MeetingBookingQueryDto>>>
{
    public override void Configure()
    {
        Tags("Meeting");
        Description(b => b.AutoTagOverride("Meeting").WithSummary("获取会议室预订分页列表"));
        Get("/api/admin/meeting/bookings");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.MeetingBookingView);
    }

    public override async Task HandleAsync(GetMeetingBookingListRequest req, CancellationToken ct)
    {
        var input = new MeetingBookingQueryInput
        {
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            MeetingRoomId = req.MeetingRoomId,
            BookerId = req.BookerId,
            StartFrom = req.StartFrom,
            StartTo = req.StartTo,
        };
        var result = await query.GetPagedAsync(input, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
