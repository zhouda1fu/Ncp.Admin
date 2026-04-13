using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.LeaveRequestAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Leaves;

/// <summary>
/// 请假申请列表请求
/// </summary>
/// <param name="PageIndex">页码</param>
/// <param name="PageSize">每页条数</param>
/// <param name="ApplicantId">申请人</param>
/// <param name="Status">状态</param>
/// <param name="LeaveType">请假类型</param>
/// <param name="StartDateFrom">开始日期起</param>
/// <param name="StartDateTo">开始日期止</param>
public record GetLeaveRequestListRequest(
    int PageIndex = 1,
    int PageSize = 20,
    UserId? ApplicantId = null,
    LeaveRequestStatus? Status = null,
    LeaveType? LeaveType = null,
    DateOnly? StartDateFrom = null,
    DateOnly? StartDateTo = null);

public class GetLeaveRequestListEndpoint(LeaveRequestQuery query) : Endpoint<GetLeaveRequestListRequest, ResponseData<PagedData<LeaveRequestQueryDto>>>
{
    public override void Configure()
    {
        Tags("Leave");
        Description(b => b.AutoTagOverride("Leave").WithSummary("请假申请列表请求"));
        Get("/api/admin/leave/requests");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.LeaveRequestView);
    }

    public override async Task HandleAsync(GetLeaveRequestListRequest req, CancellationToken ct)
    {
        var input = new LeaveRequestQueryInput
        {
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            ApplicantId = req.ApplicantId,
            Status = req.Status,
            LeaveType = req.LeaveType,
            StartDateFrom = req.StartDateFrom,
            StartDateTo = req.StartDateTo,
        };
        var result = await query.GetPagedAsync(input, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
