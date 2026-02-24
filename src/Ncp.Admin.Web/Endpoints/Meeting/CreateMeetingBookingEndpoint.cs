using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.MeetingAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Meeting;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Meeting;

/// <summary>
/// 创建会议室预订请求（会议室、主题、开始/结束时间）
/// </summary>
public class CreateMeetingBookingRequest
{
    /// <summary>
    /// 会议室 ID
    /// </summary>
    public Guid MeetingRoomId { get; set; }
    /// <summary>
    /// 会议主题
    /// </summary>
    public string Title { get; set; } = "";
    /// <summary>
    /// 开始时间，ISO8601 格式
    /// </summary>
    public string StartAt { get; set; } = "";
    /// <summary>
    /// 结束时间，ISO8601 格式
    /// </summary>
    public string EndAt { get; set; } = "";
}

/// <summary>
/// 创建会议室预订（当前用户为预订人，校验时段冲突）
/// </summary>
public class CreateMeetingBookingEndpoint(IMediator mediator) : Endpoint<CreateMeetingBookingRequest, ResponseData<CreateMeetingBookingResponse>>
{
    public override void Configure()
    {
        Tags("Meeting");
        Post("/api/admin/meeting/bookings");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.MeetingBookingCreate);
    }

    public override async Task HandleAsync(CreateMeetingBookingRequest req, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out var uid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        var startAt = DateTimeOffset.Parse(req.StartAt);
        var endAt = DateTimeOffset.Parse(req.EndAt);
        var cmd = new CreateMeetingBookingCommand(
            new MeetingRoomId(req.MeetingRoomId),
            new UserId(uid),
            req.Title,
            startAt,
            endAt);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateMeetingBookingResponse(id).AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 创建会议室预订响应（新建的预订 ID）
/// </summary>
public record CreateMeetingBookingResponse(MeetingBookingId Id);
