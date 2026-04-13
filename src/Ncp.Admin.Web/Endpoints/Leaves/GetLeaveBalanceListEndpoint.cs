using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.LeaveRequestAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Leaves;

/// <summary>
/// 请假余额列表请求
/// </summary>
/// <param name="PageIndex">页码</param>
/// <param name="PageSize">每页条数</param>
/// <param name="UserId">用户</param>
/// <param name="Year">年度</param>
/// <param name="LeaveType">请假类型</param>
public record GetLeaveBalanceListRequest(
    int PageIndex = 1,
    int PageSize = 20,
    UserId? UserId = null,
    int? Year = null,
    LeaveType? LeaveType = null);

public class GetLeaveBalanceListEndpoint(LeaveBalanceQuery query) : Endpoint<GetLeaveBalanceListRequest, ResponseData<PagedData<LeaveBalanceQueryDto>>>
{
    public override void Configure()
    {
        Tags("Leave");
        Description(b => b.AutoTagOverride("Leave").WithSummary("请假余额列表请求"));
        Get("/api/admin/leave/balances");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.LeaveBalanceView);
    }

    public override async Task HandleAsync(GetLeaveBalanceListRequest req, CancellationToken ct)
    {
        var input = new LeaveBalanceQueryInput
        {
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            UserId = req.UserId,
            Year = req.Year,
            LeaveType = req.LeaveType,
        };
        var result = await query.GetPagedAsync(input, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
