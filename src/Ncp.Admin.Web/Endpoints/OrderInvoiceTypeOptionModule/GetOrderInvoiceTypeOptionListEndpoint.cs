using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.AppPermissions;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Endpoints.OrderInvoiceTypeOptionModule;

public class GetOrderInvoiceTypeOptionListEndpoint(OrderInvoiceTypeOptionQuery query)
    : EndpointWithoutRequest<ResponseData<IReadOnlyList<OrderInvoiceTypeOptionDto>>>
{
    public override void Configure()
    {
        Tags("OrderInvoiceTypeOption");
        Get("/api/admin/order-invoice-type-options");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrderView);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await query.GetListAsync(ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}

