using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ExpenseAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Expenses;

/// <summary>
/// 报销单列表请求（分页与筛选）
/// </summary>
/// <param name="PageIndex">页码</param>
/// <param name="PageSize">每页条数</param>
/// <param name="ApplicantId">申请人</param>
/// <param name="Status">状态</param>
public record GetExpenseClaimListRequest(
    int PageIndex = 1,
    int PageSize = 20,
    UserId? ApplicantId = null,
    ExpenseClaimStatus? Status = null);

/// <summary>
/// 获取报销单分页列表
/// </summary>
public class GetExpenseClaimListEndpoint(ExpenseClaimQuery query)
    : Endpoint<GetExpenseClaimListRequest, ResponseData<PagedData<ExpenseClaimQueryDto>>>
{
    public override void Configure()
    {
        Tags("Expense");
        Description(b => b.AutoTagOverride("Expense").WithSummary("获取报销单分页列表"));
        Get("/api/admin/expense/claims");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ExpenseClaimView);
    }

    public override async Task HandleAsync(GetExpenseClaimListRequest req, CancellationToken ct)
    {
        var input = new ExpenseClaimQueryInput
        {
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            ApplicantId = req.ApplicantId,
            Status = req.Status,
        };
        var result = await query.GetPagedAsync(input, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
