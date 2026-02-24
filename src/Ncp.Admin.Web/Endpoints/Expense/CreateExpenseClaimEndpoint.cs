using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ExpenseAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Expense;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Expense;

/// <summary>
/// 报销明细项请求（类型、金额、说明、发票链接）
/// </summary>
public class ExpenseItemRequest
{
    /// <summary>
    /// 费用类型：0 差旅 1 餐饮 2 住宿 3 办公 4 其他
    /// </summary>
    public int Type { get; set; }
    /// <summary>
    /// 金额
    /// </summary>
    public decimal Amount { get; set; }
    /// <summary>
    /// 说明
    /// </summary>
    public string Description { get; set; } = "";
    /// <summary>
    /// 发票链接（可选）
    /// </summary>
    public string? InvoiceUrl { get; set; }
}

/// <summary>
/// 创建报销单请求（明细列表，申请人取当前用户）
/// </summary>
public class CreateExpenseClaimRequest
{
    /// <summary>
    /// 报销明细列表，至少一条
    /// </summary>
    public List<ExpenseItemRequest> Items { get; set; } = [];
}

/// <summary>
/// 创建报销单（草稿，含多条明细），申请人为当前登录用户
/// </summary>
public class CreateExpenseClaimEndpoint(IMediator mediator) : Endpoint<CreateExpenseClaimRequest, ResponseData<CreateExpenseClaimResponse>>
{
    public override void Configure()
    {
        Tags("Expense");
        Post("/api/admin/expense/claims");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ExpenseClaimCreate);
    }

    public override async Task HandleAsync(CreateExpenseClaimRequest req, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var userName = User.FindFirstValue(ClaimTypes.Name) ?? "";
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out var uid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        var items = req.Items.Select(i => new ExpenseItemInput((ExpenseType)i.Type, i.Amount, i.Description, i.InvoiceUrl)).ToList();
        var cmd = new CreateExpenseClaimCommand(new UserId(uid), userName, items);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateExpenseClaimResponse(id).AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 创建报销单响应（新建的报销单 ID）
/// </summary>
public record CreateExpenseClaimResponse(ExpenseClaimId Id);
