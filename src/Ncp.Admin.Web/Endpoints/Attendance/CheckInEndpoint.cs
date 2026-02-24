using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.AttendanceAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Attendance;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Attendance;

/// <summary>
/// 考勤签到请求（打卡来源与地点）
/// </summary>
public class CheckInRequest
{
    /// <summary>
    /// 打卡来源：0 GPS 1 WiFi 2 手动
    /// </summary>
    public int Source { get; set; } = (int)AttendanceSource.Manual;
    /// <summary>
    /// 打卡地点（可选）
    /// </summary>
    public string? Location { get; set; }
}

/// <summary>
/// 考勤签到（当前用户，防重复打卡）
/// </summary>
public class CheckInEndpoint(IMediator mediator) : Endpoint<CheckInRequest, ResponseData<CheckInResponse>>
{
    public override void Configure()
    {
        Tags("Attendance");
        Post("/api/admin/attendance/check-in");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.AttendanceCheckIn);
    }

    public override async Task HandleAsync(CheckInRequest req, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out var uid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        var cmd = new CheckInCommand(new UserId(uid), (AttendanceSource)req.Source, req.Location);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CheckInResponse(id).AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 考勤签到响应（新建的考勤记录 ID）
/// </summary>
public record CheckInResponse(AttendanceRecordId Id);
