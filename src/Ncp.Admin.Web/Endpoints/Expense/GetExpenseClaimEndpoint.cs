using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ExpenseAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Expense;

/// <summary>
/// 报销单详情请求（路由 {id}）
/// </summary>
/// <param name="Id">报销单 ID</param>

public record GetExpenseClaimRequest(ExpenseClaimId Id);

/// <summary>
/// 按 ID 获取报销单详情（含明细）
/// </summary>
public class GetExpenseClaimEndpoint(ExpenseClaimQuery query) : Endpoint<GetExpenseClaimRequest, ResponseData<ExpenseClaimQueryDto>>
{
    public override void Configure()
    {
        Tags("Expense");
        Get("/api/admin/expense/claims/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ExpenseClaimView);
    }

    public override async Task HandleAsync(GetExpenseClaimRequest req, CancellationToken ct)
    {
        var result = await query.GetByIdAsync(req.Id, ct);
        if (result == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
