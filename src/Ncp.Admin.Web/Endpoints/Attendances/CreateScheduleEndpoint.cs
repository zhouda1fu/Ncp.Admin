using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.AttendanceAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Attendances;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Attendances;

/// <summary>
/// 创建排班请求
/// </summary>
/// <param name="UserId">排班用户 ID</param>
/// <param name="WorkDate">工作日期，格式 YYYY-MM-DD</param>
/// <param name="StartTime">上班时间，格式 HH:mm</param>
/// <param name="EndTime">下班时间，格式 HH:mm</param>
/// <param name="ShiftName">班次名称（可选）</param>
public record CreateScheduleRequest(UserId UserId, string WorkDate, string StartTime, string EndTime, string? ShiftName);

/// <summary>
/// 创建排班
/// </summary>
public class CreateScheduleEndpoint(IMediator mediator) : Endpoint<CreateScheduleRequest, ResponseData<CreateScheduleResponse>>
{
    public override void Configure()
    {
        Tags("Attendance");
        Description(b => b.AutoTagOverride("Attendance").WithSummary("创建排班"));
        Post("/api/admin/attendance/schedules");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ScheduleEdit);
    }

    public override async Task HandleAsync(CreateScheduleRequest req, CancellationToken ct)
    {
        var workDate = DateOnly.Parse(req.WorkDate);
        var startTime = TimeOnly.Parse(req.StartTime);
        var endTime = TimeOnly.Parse(req.EndTime);
        var cmd = new CreateScheduleCommand(req.UserId, workDate, startTime, endTime, req.ShiftName);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateScheduleResponse(id).AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 创建排班响应（新建的排班 ID）
/// </summary>
public record CreateScheduleResponse(ScheduleId Id);
