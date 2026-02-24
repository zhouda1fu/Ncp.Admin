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
/// 创建排班请求
/// </summary>
public class CreateScheduleRequest
{
    /// <summary>
    /// 排班用户 ID
    /// </summary>
    public long UserId { get; set; }
    /// <summary>
    /// 工作日期，格式 YYYY-MM-DD
    /// </summary>
    public string WorkDate { get; set; } = "";
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
/// 创建排班
/// </summary>
public class CreateScheduleEndpoint(IMediator mediator) : Endpoint<CreateScheduleRequest, ResponseData<CreateScheduleResponse>>
{
    public override void Configure()
    {
        Tags("Attendance");
        Post("/api/admin/attendance/schedules");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ScheduleEdit);
    }

    public override async Task HandleAsync(CreateScheduleRequest req, CancellationToken ct)
    {
        var workDate = DateOnly.Parse(req.WorkDate);
        var startTime = TimeOnly.Parse(req.StartTime);
        var endTime = TimeOnly.Parse(req.EndTime);
        var cmd = new CreateScheduleCommand(new UserId(req.UserId), workDate, startTime, endTime, req.ShiftName);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateScheduleResponse(id).AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 创建排班响应（新建的排班 ID）
/// </summary>
public record CreateScheduleResponse(ScheduleId Id);
