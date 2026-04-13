using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Meetings;

/// <summary>
/// 会议室列表请求（分页与筛选）
/// </summary>
/// <param name="PageIndex">页码</param>
/// <param name="PageSize">每页条数</param>
/// <param name="Name">名称关键字</param>
/// <param name="Status">状态（0 禁用 1 可用）</param>
public record GetMeetingRoomListRequest(
    int PageIndex = 1,
    int PageSize = 20,
    string? Name = null,
    int? Status = null);

/// <summary>
/// 获取会议室分页列表
/// </summary>
public class GetMeetingRoomListEndpoint(MeetingRoomQuery query)
    : Endpoint<GetMeetingRoomListRequest, ResponseData<PagedData<MeetingRoomQueryDto>>>
{
    public override void Configure()
    {
        Tags("Meeting");
        Description(b => b.AutoTagOverride("Meeting").WithSummary("获取会议室分页列表"));
        Get("/api/admin/meeting/rooms");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.MeetingRoomView);
    }

    public override async Task HandleAsync(GetMeetingRoomListRequest req, CancellationToken ct)
    {
        var input = new MeetingRoomQueryInput
        {
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            Name = req.Name,
            Status = req.Status,
        };
        var result = await query.GetPagedAsync(input, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
