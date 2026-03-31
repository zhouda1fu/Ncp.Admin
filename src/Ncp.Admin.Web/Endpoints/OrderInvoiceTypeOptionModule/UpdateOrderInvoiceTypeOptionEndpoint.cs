using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.OrderInvoiceTypeOptionAggregate;
using Ncp.Admin.Web.Application.Commands.OrderInvoiceTypeOptionModule;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.OrderInvoiceTypeOptionModule;

public record UpdateOrderInvoiceTypeOptionRequest(string Name, int TypeValue, int SortOrder = 0);

public class UpdateOrderInvoiceTypeOptionEndpoint(IMediator mediator)
    : Endpoint<UpdateOrderInvoiceTypeOptionRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("OrderInvoiceTypeOption");
        Put("/api/admin/order-invoice-type-options/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrderEdit); // Reuse OrderEdit permission
    }

    public override async Task HandleAsync(UpdateOrderInvoiceTypeOptionRequest req, CancellationToken ct)
    {
        var id = new OrderInvoiceTypeOptionId(Route<long>("id"));
        await mediator.Send(new UpdateOrderInvoiceTypeOptionCommand(
            id, req.Name, req.TypeValue, req.SortOrder), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
