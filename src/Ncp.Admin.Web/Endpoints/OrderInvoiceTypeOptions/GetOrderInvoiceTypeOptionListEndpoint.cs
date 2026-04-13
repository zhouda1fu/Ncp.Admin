using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.AppPermissions;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Endpoints.OrdersInvoiceTypeOptions;

public class GetOrderInvoiceTypeOptionListEndpoint(OrderInvoiceTypeOptionQuery query)
    : EndpointWithoutRequest<ResponseData<IReadOnlyList<OrderInvoiceTypeOptionDto>>>
{
    public override void Configure()
    {
        Tags("OrderInvoiceTypeOption");
        Description(b => b.AutoTagOverride("OrderInvoiceTypeOption").WithSummary("获取订单发票类型选项列表"));
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

