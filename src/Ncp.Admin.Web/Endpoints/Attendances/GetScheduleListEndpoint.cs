using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Attendances;

/// <summary>
/// 排班列表请求（分页与筛选）
/// </summary>
/// <param name="PageIndex">页码</param>
/// <param name="PageSize">每页条数</param>
/// <param name="UserId">用户</param>
/// <param name="WorkDateFrom">工作日期起</param>
/// <param name="WorkDateTo">工作日期止</param>
public record GetScheduleListRequest(
    int PageIndex = 1,
    int PageSize = 20,
    UserId? UserId = null,
    DateOnly? WorkDateFrom = null,
    DateOnly? WorkDateTo = null);

/// <summary>
/// 获取排班分页列表
/// </summary>
public class GetScheduleListEndpoint(ScheduleQuery query)
    : Endpoint<GetScheduleListRequest, ResponseData<PagedData<ScheduleQueryDto>>>
{
    public override void Configure()
    {
        Tags("Attendance");
        Description(b => b.AutoTagOverride("Attendance").WithSummary("获取排班分页列表"));
        Get("/api/admin/attendance/schedules");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ScheduleView);
    }

    public override async Task HandleAsync(GetScheduleListRequest req, CancellationToken ct)
    {
        var input = new ScheduleQueryInput
        {
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            UserId = req.UserId,
            WorkDateFrom = req.WorkDateFrom,
            WorkDateTo = req.WorkDateTo,
        };
        var result = await query.GetPagedAsync(input, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
