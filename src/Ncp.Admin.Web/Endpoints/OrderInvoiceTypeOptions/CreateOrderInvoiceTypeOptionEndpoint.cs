using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.OrderInvoiceTypeOptionAggregate;
using Ncp.Admin.Web.Application.Commands.OrderInvoiceTypeOptions;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.OrdersInvoiceTypeOptions;

public record CreateOrderInvoiceTypeOptionRequest(string Name, int TypeValue, int SortOrder = 0);

public record CreateOrderInvoiceTypeOptionResponse(OrderInvoiceTypeOptionId Id);

public class CreateOrderInvoiceTypeOptionEndpoint(IMediator mediator)
    : Endpoint<CreateOrderInvoiceTypeOptionRequest, ResponseData<CreateOrderInvoiceTypeOptionResponse>>
{
    public override void Configure()
    {
        Tags("OrderInvoiceTypeOption");
        Description(b => b.AutoTagOverride("OrderInvoiceTypeOption").WithSummary("创建订单发票类型选项"));
        Post("/api/admin/order-invoice-type-options");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrderCreate); // Reuse OrderCreate permission
    }

    public override async Task HandleAsync(CreateOrderInvoiceTypeOptionRequest req, CancellationToken ct)
    {
        var id = await mediator.Send(new CreateOrderInvoiceTypeOptionCommand(
            req.Name, req.TypeValue, req.SortOrder), ct);
        await Send.OkAsync(new CreateOrderInvoiceTypeOptionResponse(id).AsResponseData(), cancellation: ct);
    }
}
