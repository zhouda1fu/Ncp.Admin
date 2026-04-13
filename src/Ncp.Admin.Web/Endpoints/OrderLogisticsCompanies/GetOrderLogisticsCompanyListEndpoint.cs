using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.AppPermissions;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Endpoints.OrdersLogisticsCompanies;

public class GetOrderLogisticsCompanyListEndpoint(OrderLogisticsCompanyQuery query)
    : EndpointWithoutRequest<ResponseData<IReadOnlyList<OrderLogisticsCompanyDto>>>
{
    public override void Configure()
    {
        Tags("OrderLogisticsCompany");
        Description(b => b.AutoTagOverride("OrderLogisticsCompany").WithSummary("获取订单物流公司列表"));
        Get("/api/admin/order-logistics-companies");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrderView);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await query.GetListAsync(ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
