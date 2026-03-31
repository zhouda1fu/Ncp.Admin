using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Web.Application.Commands.ContractModule;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Contract;

/// <summary>
/// 新增合同发票请求
/// </summary>
public record AddContractInvoiceRequest(
    int Type,
    string InvoiceNumber,
    decimal TaxRate,
    decimal AmountExclTax,
    string Source,
    bool Status,
    string Title,
    decimal TaxAmount,
    decimal InvoicedAmount,
    string? Handler = null,
    DateTimeOffset? BillingDate = null,
    string? Remarks = null,
    string? AttachmentStorageKey = null);

/// <summary>
/// 新增合同发票
/// </summary>
public class AddContractInvoiceEndpoint(IMediator mediator) : Endpoint<AddContractInvoiceRequest, ResponseData<AddContractInvoiceResponse>>
{
    public override void Configure()
    {
        Tags("Contract");
        Post("/api/admin/contracts/{id}/invoices");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ContractEdit);
    }

    public override async Task HandleAsync(AddContractInvoiceRequest req, CancellationToken ct)
    {
        var contractId = new ContractId(Route<Guid>("id"));
        var type = (InvoiceType)req.Type;
        var billingDate = req.BillingDate ?? DateTimeOffset.UtcNow;
        var cmd = new AddContractInvoiceCommand(
            contractId, type, req.InvoiceNumber, req.TaxRate, req.AmountExclTax, req.Source,
            req.Status, req.Title, req.TaxAmount, req.InvoicedAmount, req.Handler ?? string.Empty,
            billingDate, req.Remarks ?? string.Empty, req.AttachmentStorageKey ?? string.Empty);
        var invoiceId = await mediator.Send(cmd, ct);
        await Send.OkAsync(new AddContractInvoiceResponse(invoiceId).AsResponseData(), cancellation: ct);
    }
}

/// <summary>
/// 新增合同发票响应
/// </summary>
public record AddContractInvoiceResponse(ContractInvoiceId Id);
