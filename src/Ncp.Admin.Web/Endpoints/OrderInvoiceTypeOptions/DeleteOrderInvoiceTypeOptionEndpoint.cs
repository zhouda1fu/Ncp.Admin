using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.OrderInvoiceTypeOptionAggregate;
using Ncp.Admin.Web.Application.Commands.OrderInvoiceTypeOptions;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.OrdersInvoiceTypeOptions;

public class DeleteOrderInvoiceTypeOptionEndpoint(IMediator mediator)
    : EndpointWithoutRequest<ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("OrderInvoiceTypeOption");
        Description(b => b.AutoTagOverride("OrderInvoiceTypeOption").WithSummary("删除订单发票类型选项"));
        Delete("/api/admin/order-invoice-type-options/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrderDelete); // Reuse OrderDelete permission
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = Route<long>("id");
        await mediator.Send(new DeleteOrderInvoiceTypeOptionCommand(new OrderInvoiceTypeOptionId(id)), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
