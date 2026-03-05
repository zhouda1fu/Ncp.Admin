using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.AttendanceAggregate;
using Ncp.Admin.Web.Application.Commands.Attendance;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Attendance;

/// <summary>
/// 更新排班请求
/// </summary>
/// <param name="Id">排班 ID</param>
/// <param name="StartTime">上班时间，格式 HH:mm</param>
/// <param name="EndTime">下班时间，格式 HH:mm</param>
/// <param name="ShiftName">班次名称（可选）</param>
public record UpdateScheduleRequest(ScheduleId Id, string StartTime, string EndTime, string? ShiftName);

/// <summary>
/// 更新排班（班次时间与名称）
/// </summary>
public class UpdateScheduleEndpoint(IMediator mediator) : Endpoint<UpdateScheduleRequest>
{
    public override void Configure()
    {
        Tags("Attendance");
        Put("/api/admin/attendance/schedules/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ScheduleEdit);
    }

    public override async Task HandleAsync(UpdateScheduleRequest req, CancellationToken ct)
    {
        var startTime = TimeOnly.Parse(req.StartTime);
        var endTime = TimeOnly.Parse(req.EndTime);
        var cmd = new UpdateScheduleCommand(req.Id, startTime, endTime, req.ShiftName);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(ct);
    }
}
