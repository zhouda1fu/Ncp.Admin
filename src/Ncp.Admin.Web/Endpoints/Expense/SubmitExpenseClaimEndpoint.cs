using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ExpenseAggregate;
using Ncp.Admin.Web.Application.Commands.Expense;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Expense;

/// <summary>
/// 提交报销单请求（路由 {id} 为报销单 ID）
/// </summary>
public class SubmitExpenseClaimRequest
{
    /// <summary>
    /// 报销单 ID
    /// </summary>
    public Guid Id { get; set; }
}

/// <summary>
/// 提交报销单（仅草稿可提交）
/// </summary>
public class SubmitExpenseClaimEndpoint(IMediator mediator) : Endpoint<SubmitExpenseClaimRequest>
{
    public override void Configure()
    {
        Tags("Expense");
        Post("/api/admin/expense/claims/{id}/submit");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ExpenseClaimSubmit);
    }

    public override async Task HandleAsync(SubmitExpenseClaimRequest req, CancellationToken ct)
    {
        await mediator.Send(new SubmitExpenseClaimCommand(new ExpenseClaimId(req.Id)), ct);
        await Send.OkAsync(ct);
    }
}
