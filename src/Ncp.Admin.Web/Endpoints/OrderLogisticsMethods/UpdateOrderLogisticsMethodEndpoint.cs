using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.OrderLogisticsMethodAggregate;
using Ncp.Admin.Web.Application.Commands.OrderLogisticsMethods;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.OrdersLogisticsMethods;

public record UpdateOrderLogisticsMethodRequest(string Name, int TypeValue, int Sort = 0);

public class UpdateOrderLogisticsMethodEndpoint(IMediator mediator)
    : Endpoint<UpdateOrderLogisticsMethodRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("OrderLogisticsMethod");
        Description(b => b.AutoTagOverride("OrderLogisticsMethod").WithSummary("更新订单物流方式"));
        Put("/api/admin/order-logistics-methods/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrderEdit);
    }

    public override async Task HandleAsync(UpdateOrderLogisticsMethodRequest req, CancellationToken ct)
    {
        var id = new OrderLogisticsMethodId(Route<Guid>("id"));
        await mediator.Send(new UpdateOrderLogisticsMethodCommand(
            id, req.Name, req.TypeValue, req.Sort), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
