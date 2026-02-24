using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.MeetingAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Meeting;

/// <summary>
/// 会议室预订列表请求（分页与筛选）
/// </summary>
public class GetMeetingBookingListRequest : PageRequest
{
    /// <summary>
    /// 会议室 ID 筛选
    /// </summary>
    public Guid? MeetingRoomId { get; set; }
    /// <summary>
    /// 预订人 ID 筛选
    /// </summary>
    public long? BookerId { get; set; }
    /// <summary>
    /// 开始时间起
    /// </summary>
    public DateTimeOffset? StartFrom { get; set; }
    /// <summary>
    /// 开始时间止
    /// </summary>
    public DateTimeOffset? StartTo { get; set; }
}

/// <summary>
/// 获取会议室预订分页列表
/// </summary>
public class GetMeetingBookingListEndpoint(MeetingBookingQuery query)
    : Endpoint<GetMeetingBookingListRequest, ResponseData<PagedData<MeetingBookingQueryDto>>>
{
    public override void Configure()
    {
        Tags("Meeting");
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
            MeetingRoomId = req.MeetingRoomId.HasValue ? new MeetingRoomId(req.MeetingRoomId.Value) : null,
            BookerId = req.BookerId.HasValue ? new UserId(req.BookerId.Value) : null,
            StartFrom = req.StartFrom,
            StartTo = req.StartTo,
        };
        var result = await query.GetPagedAsync(input, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
