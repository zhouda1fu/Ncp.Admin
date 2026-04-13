using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.AttendanceAggregate;
using Ncp.Admin.Web.Application.Commands.Attendances;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Attendances;

/// <summary>
/// 考勤签退请求
/// </summary>
/// <param name="Id">考勤记录 ID</param>
public record CheckOutRequest(AttendanceRecordId Id);

/// <summary>
/// 考勤签退（按考勤记录 ID）
/// </summary>
public class CheckOutEndpoint(IMediator mediator) : Endpoint<CheckOutRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Attendance");
        Description(b => b.AutoTagOverride("Attendance").WithSummary("考勤签退（按考勤记录 ID）"));
        Post("/api/admin/attendance/check-out");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.AttendanceCheckIn);
    }

    public override async Task HandleAsync(CheckOutRequest req, CancellationToken ct)
    {
        var cmd = new CheckOutCommand(req.Id);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
