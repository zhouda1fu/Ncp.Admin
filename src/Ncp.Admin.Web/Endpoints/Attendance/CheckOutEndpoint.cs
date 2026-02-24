using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.AttendanceAggregate;
using Ncp.Admin.Web.Application.Commands.Attendance;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Attendance;

/// <summary>
/// 考勤签退请求（考勤记录 ID）
/// </summary>
public class CheckOutRequest
{
    /// <summary>
    /// 考勤记录 ID
    /// </summary>
    public Guid Id { get; set; }
}

/// <summary>
/// 考勤签退（按考勤记录 ID）
/// </summary>
public class CheckOutEndpoint(IMediator mediator) : Endpoint<CheckOutRequest>
{
    public override void Configure()
    {
        Tags("Attendance");
        Post("/api/admin/attendance/check-out");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.AttendanceCheckIn);
    }

    public override async Task HandleAsync(CheckOutRequest req, CancellationToken ct)
    {
        var cmd = new CheckOutCommand(new AttendanceRecordId(req.Id));
        await mediator.Send(cmd, ct);
        await Send.OkAsync(ct);
    }
}
