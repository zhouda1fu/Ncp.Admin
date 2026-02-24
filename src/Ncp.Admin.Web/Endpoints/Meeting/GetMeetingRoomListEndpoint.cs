using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Meeting;

/// <summary>
/// 会议室列表请求（继承分页与筛选）
/// </summary>
public class GetMeetingRoomListRequest : MeetingRoomQueryInput { }

/// <summary>
/// 获取会议室分页列表
/// </summary>
public class GetMeetingRoomListEndpoint(MeetingRoomQuery query)
    : Endpoint<GetMeetingRoomListRequest, ResponseData<PagedData<MeetingRoomQueryDto>>>
{
    public override void Configure()
    {
        Tags("Meeting");
        Get("/api/admin/meeting/rooms");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.MeetingRoomView);
    }

    public override async Task HandleAsync(GetMeetingRoomListRequest req, CancellationToken ct)
    {
        var result = await query.GetPagedAsync(req, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
