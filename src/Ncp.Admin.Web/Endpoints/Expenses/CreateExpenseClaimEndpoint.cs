using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ExpenseAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Expenses;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Expenses;

/// <summary>
/// 报销明细项请求（类型、金额、说明、发票链接）
/// </summary>
/// <param name="Type">费用类型：0 差旅 1 餐饮 2 住宿 3 办公 4 其他</param>
/// <param name="Amount">金额</param>
/// <param name="Description">说明</param>
/// <param name="InvoiceUrl">发票链接</param>
public record ExpenseItemRequest(int Type, decimal Amount, string Description, string? InvoiceUrl = null);

/// <summary>
/// 创建报销单请求（明细列表，申请人取当前用户）
/// </summary>
/// <param name="Items">报销明细，至少一条</param>
public record CreateExpenseClaimRequest(List<ExpenseItemRequest> Items);

/// <summary>
/// 创建报销单（草稿，含多条明细），申请人为当前登录用户
/// </summary>
public class CreateExpenseClaimEndpoint(IMediator mediator) : Endpoint<CreateExpenseClaimRequest, ResponseData<CreateExpenseClaimResponse>>
{
    public override void Configure()
    {
        Tags("Expense");
        Description(b => b.AutoTagOverride("Expense").WithSummary("创建报销单（草稿，含多条明细），申请人为当前登录用户"));
        Post("/api/admin/expense/claims");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ExpenseClaimCreate);
    }

    public override async Task HandleAsync(CreateExpenseClaimRequest req, CancellationToken ct)
    {
        if (!User.TryGetUserId(out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var userName = User.GetUserDisplayName();
        var items = req.Items.Select(i => new ExpenseItemInput((ExpenseType)i.Type, i.Amount, i.Description, i.InvoiceUrl)).ToList();
        var cmd = new CreateExpenseClaimCommand(userId, userName, items);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateExpenseClaimResponse(id).AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 创建报销单响应（新建的报销单 ID）
/// </summary>
/// <param name="Id">报销单 ID</param>
public record CreateExpenseClaimResponse(ExpenseClaimId Id);
