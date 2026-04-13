using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Web.Application.Commands.Contracts;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Contracts;

/// <summary>
/// 删除合同发票
/// </summary>
public class RemoveContractInvoiceEndpoint(IMediator mediator) : EndpointWithoutRequest<ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Contract");
        Description(b => b.AutoTagOverride("Contract").WithSummary("删除合同发票"));
        Delete("/api/admin/contracts/{id}/invoices/{invoiceId}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ContractEdit);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var contractId = new ContractId(Route<Guid>("id"));
        var invoiceId = new ContractInvoiceId(Route<Guid>("invoiceId"));
        await mediator.Send(new RemoveContractInvoiceCommand(contractId, invoiceId), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
