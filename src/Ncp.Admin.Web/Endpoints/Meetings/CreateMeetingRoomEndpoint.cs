using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.MeetingAggregate;
using Ncp.Admin.Web.Application.Commands.Meetings;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Meetings;

/// <summary>
/// 创建会议室请求
/// </summary>
/// <param name="Name">会议室名称</param>
/// <param name="Location">位置（可选）</param>
/// <param name="Capacity">容纳人数</param>
/// <param name="Equipment">设备说明（可选）</param>
public record CreateMeetingRoomRequest(string Name, int Capacity, string? Location = null, string? Equipment = null);

/// <summary>
/// 创建会议室
/// </summary>
public class CreateMeetingRoomEndpoint(IMediator mediator) : Endpoint<CreateMeetingRoomRequest, ResponseData<CreateMeetingRoomResponse>>
{
    public override void Configure()
    {
        Tags("Meeting");
        Description(b => b.AutoTagOverride("Meeting").WithSummary("创建会议室"));
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
public record CreateMeetingRoomResponse(MeetingRoomId Id);
