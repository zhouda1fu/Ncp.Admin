using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ExpenseAggregate;
using Ncp.Admin.Web.Application.Commands.Expenses;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Expenses;

/// <summary>
/// 提交报销单请求
/// </summary>
/// <param name="Id">报销单 ID</param>
public record SubmitExpenseClaimRequest(ExpenseClaimId Id);

/// <summary>
/// 提交报销单（仅草稿可提交）
/// </summary>
public class SubmitExpenseClaimEndpoint(IMediator mediator) : Endpoint<SubmitExpenseClaimRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Expense");
        Description(b => b.AutoTagOverride("Expense").WithSummary("提交报销单（仅草稿可提交）"));
        Post("/api/admin/expense/claims/{id}/submit");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ExpenseClaimSubmit);
    }

    public override async Task HandleAsync(SubmitExpenseClaimRequest req, CancellationToken ct)
    {
        await mediator.Send(new SubmitExpenseClaimCommand(req.Id), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
