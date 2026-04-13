using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Attendances;

/// <summary>
/// 考勤记录列表请求（分页与筛选）
/// </summary>
/// <param name="PageIndex">页码</param>
/// <param name="PageSize">每页条数</param>
/// <param name="UserId">用户</param>
/// <param name="DateFrom">签到日期起</param>
/// <param name="DateTo">签到日期止</param>
public record GetAttendanceRecordListRequest(
    int PageIndex = 1,
    int PageSize = 20,
    UserId? UserId = null,
    DateOnly? DateFrom = null,
    DateOnly? DateTo = null);

/// <summary>
/// 获取考勤记录分页列表
/// </summary>
public class GetAttendanceRecordListEndpoint(AttendanceRecordQuery query)
    : Endpoint<GetAttendanceRecordListRequest, ResponseData<PagedData<AttendanceRecordQueryDto>>>
{
    public override void Configure()
    {
        Tags("Attendance");
        Description(b => b.AutoTagOverride("Attendance").WithSummary("获取考勤记录分页列表"));
        Get("/api/admin/attendance/records");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.AttendanceRecordView);
    }

    public override async Task HandleAsync(GetAttendanceRecordListRequest req, CancellationToken ct)
    {
        var input = new AttendanceRecordQueryInput
        {
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            UserId = req.UserId,
            DateFrom = req.DateFrom,
            DateTo = req.DateTo,
        };
        var result = await query.GetPagedAsync(input, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
