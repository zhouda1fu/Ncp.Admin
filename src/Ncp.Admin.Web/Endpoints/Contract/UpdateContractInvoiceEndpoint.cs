using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Web.Application.Commands.ContractModule;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Contract;

/// <summary>
/// 更新合同发票请求
/// </summary>
public record UpdateContractInvoiceRequest(
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
/// 更新合同发票
/// </summary>
public class UpdateContractInvoiceEndpoint(IMediator mediator) : Endpoint<UpdateContractInvoiceRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Contract");
        Put("/api/admin/contracts/{id}/invoices/{invoiceId}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ContractEdit);
    }

    public override async Task HandleAsync(UpdateContractInvoiceRequest req, CancellationToken ct)
    {
        var contractId = new ContractId(Route<Guid>("id"));
        var invoiceId = new ContractInvoiceId(Route<Guid>("invoiceId"));
        var type = (InvoiceType)req.Type;
        var billingDate = req.BillingDate ?? DateTimeOffset.UtcNow;
        var cmd = new UpdateContractInvoiceCommand(
            contractId, invoiceId, type, req.InvoiceNumber, req.TaxRate, req.AmountExclTax, req.Source,
            req.Status, req.Title, req.TaxAmount, req.InvoicedAmount, req.Handler ?? string.Empty,
            billingDate, req.Remarks ?? string.Empty, req.AttachmentStorageKey ?? string.Empty);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
