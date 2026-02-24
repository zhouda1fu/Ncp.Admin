using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.AttendanceAggregate;
using Ncp.Admin.Web.Application.Commands.Attendance;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Attendance;

/// <summary>
/// 更新排班请求（路由 {id} 与 Body 时间、班次名）
/// </summary>
public class UpdateScheduleRequest
{
    /// <summary>
    /// 排班 ID（路由参数）
    /// </summary>
    public Guid Id { get; set; }
    /// <summary>
    /// 上班时间，格式 HH:mm
    /// </summary>
    public string StartTime { get; set; } = "";
    /// <summary>
    /// 下班时间，格式 HH:mm
    /// </summary>
    public string EndTime { get; set; } = "";
    /// <summary>
    /// 班次名称（可选）
    /// </summary>
    public string? ShiftName { get; set; }
}

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
        var cmd = new UpdateScheduleCommand(new ScheduleId(req.Id), startTime, endTime, req.ShiftName);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(ct);
    }
}
