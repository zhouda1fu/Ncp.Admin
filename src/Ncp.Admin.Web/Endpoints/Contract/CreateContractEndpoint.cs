using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Contract;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Contract;

/// <summary>
/// 创建合同请求（全部必传，ID 使用强类型）
/// </summary>
public record CreateContractRequest(
    string Code,
    string Title,
    string PartyA,
    string PartyB,
    decimal Amount,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string FileStorageKey,
    OrderId OrderId,
    CustomerId CustomerId,
    int ContractType,
    int IncomeExpenseType,
    DateTimeOffset SignDate,
    string Note,
    string Description,
    Guid DepartmentId,
    string BusinessManager,
    string ResponsibleProject,
    string InputCustomer,
    bool NextPaymentReminder,
    bool ContractExpiryReminder,
    int SingleDoubleProfit,
    string InvoicingInformation,
    int PaymentStatus,
    string WarrantyPeriod,
    bool IsInstallmentPayment,
    decimal AccumulatedAmount);

/// <summary>
/// 创建合同（当前用户为创建人）
/// </summary>
public class CreateContractEndpoint(IMediator mediator) : Endpoint<CreateContractRequest, ResponseData<CreateContractResponse>>
{
    public override void Configure()
    {
        Tags("Contract");
        Post("/api/admin/contracts");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ContractCreate);
    }

    public override async Task HandleAsync(CreateContractRequest req, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out var uid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        var cmd = new CreateContractCommand(req.Code, req.Title, req.PartyA, req.PartyB,
            req.Amount, req.StartDate, req.EndDate, new UserId(uid),
            req.FileStorageKey, req.OrderId, req.CustomerId, req.ContractType, req.IncomeExpenseType,
            req.SignDate, req.Note, req.Description,
            req.DepartmentId, req.BusinessManager, req.ResponsibleProject, req.InputCustomer,
            req.NextPaymentReminder, req.ContractExpiryReminder,
            req.SingleDoubleProfit, req.InvoicingInformation, req.PaymentStatus, req.WarrantyPeriod,
            req.IsInstallmentPayment, req.AccumulatedAmount);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateContractResponse(id).AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 创建合同响应
/// </summary>
public record CreateContractResponse(ContractId Id);
