using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Attendance;

/// <summary>
/// 考勤记录列表请求（继承分页与筛选）
/// </summary>
public class GetAttendanceRecordListRequest : AttendanceRecordQueryInput { }

/// <summary>
/// 获取考勤记录分页列表
/// </summary>
public class GetAttendanceRecordListEndpoint(AttendanceRecordQuery query)
    : Endpoint<GetAttendanceRecordListRequest, ResponseData<PagedData<AttendanceRecordQueryDto>>>
{
    public override void Configure()
    {
        Tags("Attendance");
        Get("/api/admin/attendance/records");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.AttendanceRecordView);
    }

    public override async Task HandleAsync(GetAttendanceRecordListRequest req, CancellationToken ct)
    {
        var result = await query.GetPagedAsync(req, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
