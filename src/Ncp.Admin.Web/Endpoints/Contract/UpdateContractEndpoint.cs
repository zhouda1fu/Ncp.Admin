using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Web.Application.Commands.Contract;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Contract;

/// <summary>
/// 更新合同请求（Id 来自路由）
/// </summary>
public record UpdateContractRequest(
    string Code,
    string Title,
    string PartyA,
    string PartyB,
    decimal Amount,
    DateTimeOffset StartDate,
    DateTimeOffset EndDate,
    string? FileStorageKey = null,
    string? OrderId = null,
    string? CustomerId = null,
    int? ContractType = null,
    int? IncomeExpenseType = null,
    DateTimeOffset? SignDate = null,
    string? Note = null,
    string? Description = null,
    Guid? DepartmentId = null,
    string? BusinessManager = null,
    string? ResponsibleProject = null,
    string? InputCustomer = null,
    bool? NextPaymentReminder = null,
    bool? ContractExpiryReminder = null,
    int? SingleDoubleProfit = null,
    string? InvoicingInformation = null,
    int? PaymentStatus = null,
    string? WarrantyPeriod = null,
    bool? IsInstallmentPayment = null,
    decimal? AccumulatedAmount = null);

/// <summary>
/// 更新合同
/// </summary>
public class UpdateContractEndpoint(IMediator mediator) : Endpoint<UpdateContractRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Contract");
        Put("/api/admin/contracts/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ContractEdit);
    }

    public override async Task HandleAsync(UpdateContractRequest req, CancellationToken ct)
    {
        var id = new ContractId(Route<Guid>("id"));
        OrderId? orderId = !string.IsNullOrWhiteSpace(req.OrderId) && Guid.TryParse(req.OrderId, out var oid) ? new OrderId(oid) : null;
        CustomerId? customerId = !string.IsNullOrWhiteSpace(req.CustomerId) && Guid.TryParse(req.CustomerId, out var cid) ? new CustomerId(cid) : null;
        var cmd = new UpdateContractCommand(id, req.Code, req.Title, req.PartyA, req.PartyB,
            req.Amount, req.StartDate, req.EndDate, req.FileStorageKey,
            orderId, customerId, req.ContractType, req.IncomeExpenseType, req.SignDate, req.Note, req.Description,
            req.DepartmentId, req.BusinessManager, req.ResponsibleProject, req.InputCustomer,
            req.NextPaymentReminder, req.ContractExpiryReminder, req.SingleDoubleProfit,
            req.InvoicingInformation, req.PaymentStatus, req.WarrantyPeriod,
            req.IsInstallmentPayment, req.AccumulatedAmount);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
