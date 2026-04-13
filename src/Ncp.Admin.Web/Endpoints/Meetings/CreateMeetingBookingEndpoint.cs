using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.MeetingAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Meetings;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Meetings;

/// <summary>
/// 创建会议室预订请求
/// </summary>
/// <param name="MeetingRoomId">会议室 ID</param>
/// <param name="Title">会议主题</param>
/// <param name="StartAt">开始时间，ISO8601 格式</param>
/// <param name="EndAt">结束时间，ISO8601 格式</param>
public record CreateMeetingBookingRequest(MeetingRoomId MeetingRoomId, string Title, string StartAt, string EndAt);

/// <summary>
/// 创建会议室预订（当前用户为预订人，校验时段冲突）
/// </summary>
public class CreateMeetingBookingEndpoint(IMediator mediator) : Endpoint<CreateMeetingBookingRequest, ResponseData<CreateMeetingBookingResponse>>
{
    public override void Configure()
    {
        Tags("Meeting");
        Description(b => b.AutoTagOverride("Meeting").WithSummary("创建会议室预订（当前用户为预订人，校验时段冲突）"));
        Post("/api/admin/meeting/bookings");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.MeetingBookingCreate);
    }

    public override async Task HandleAsync(CreateMeetingBookingRequest req, CancellationToken ct)
    {
        if (!User.TryGetUserId(out var uid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        var startAt = DateTimeOffset.Parse(req.StartAt);
        var endAt = DateTimeOffset.Parse(req.EndAt);
        var cmd = new CreateMeetingBookingCommand(
            req.MeetingRoomId,
            uid,
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
