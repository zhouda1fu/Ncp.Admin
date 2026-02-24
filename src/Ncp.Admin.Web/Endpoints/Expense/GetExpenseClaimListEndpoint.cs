using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Expense;

/// <summary>
/// 报销单列表请求（继承分页与筛选）
/// </summary>
public class GetExpenseClaimListRequest : ExpenseClaimQueryInput { }

/// <summary>
/// 获取报销单分页列表
/// </summary>
public class GetExpenseClaimListEndpoint(ExpenseClaimQuery query)
    : Endpoint<GetExpenseClaimListRequest, ResponseData<PagedData<ExpenseClaimQueryDto>>>
{
    public override void Configure()
    {
        Tags("Expense");
        Get("/api/admin/expense/claims");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ExpenseClaimView);
    }

    public override async Task HandleAsync(GetExpenseClaimListRequest req, CancellationToken ct)
    {
        var result = await query.GetPagedAsync(req, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
