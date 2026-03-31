using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.AppPermissions;
using Ncp.Admin.Web.Application.Queries;

namespace Ncp.Admin.Web.Endpoints.OrderLogisticsMethodModule;

public class GetOrderLogisticsMethodListEndpoint(OrderLogisticsMethodQuery query)
    : EndpointWithoutRequest<ResponseData<IReadOnlyList<OrderLogisticsMethodDto>>>
{
    public override void Configure()
    {
        Tags("OrderLogisticsMethod");
        Get("/api/admin/order-logistics-methods");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrderView);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var result = await query.GetListAsync(ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
