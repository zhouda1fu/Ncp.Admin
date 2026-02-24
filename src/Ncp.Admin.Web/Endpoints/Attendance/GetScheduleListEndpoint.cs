using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Attendance;

/// <summary>
/// 排班列表请求（继承分页与筛选）
/// </summary>
public class GetScheduleListRequest : ScheduleQueryInput { }

/// <summary>
/// 获取排班分页列表
/// </summary>
public class GetScheduleListEndpoint(ScheduleQuery query)
    : Endpoint<GetScheduleListRequest, ResponseData<PagedData<ScheduleQueryDto>>>
{
    public override void Configure()
    {
        Tags("Attendance");
        Get("/api/admin/attendance/schedules");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ScheduleView);
    }

    public override async Task HandleAsync(GetScheduleListRequest req, CancellationToken ct)
    {
        var result = await query.GetPagedAsync(req, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
