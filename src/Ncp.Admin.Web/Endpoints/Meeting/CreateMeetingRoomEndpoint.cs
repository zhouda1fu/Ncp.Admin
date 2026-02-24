using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Commands.Meeting;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Meeting;

/// <summary>
/// 创建会议室请求
/// </summary>
public class CreateMeetingRoomRequest
{
    /// <summary>
    /// 会议室名称
    /// </summary>
    public string Name { get; set; } = "";
    /// <summary>
    /// 位置（可选）
    /// </summary>
    public string? Location { get; set; }
    /// <summary>
    /// 容纳人数
    /// </summary>
    public int Capacity { get; set; }
    /// <summary>
    /// 设备说明（可选）
    /// </summary>
    public string? Equipment { get; set; }
}

/// <summary>
/// 创建会议室
/// </summary>
public class CreateMeetingRoomEndpoint(IMediator mediator) : Endpoint<CreateMeetingRoomRequest, ResponseData<CreateMeetingRoomResponse>>
{
    public override void Configure()
    {
        Tags("Meeting");
        Post("/api/admin/meeting/rooms");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.MeetingRoomEdit);
    }

    public override async Task HandleAsync(CreateMeetingRoomRequest req, CancellationToken ct)
    {
        var cmd = new CreateMeetingRoomCommand(req.Name, req.Location, req.Capacity, req.Equipment);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateMeetingRoomResponse(id).AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 创建会议室响应（新建的会议室 ID）
/// </summary>
public record CreateMeetingRoomResponse(Ncp.Admin.Domain.AggregatesModel.MeetingAggregate.MeetingRoomId Id);
