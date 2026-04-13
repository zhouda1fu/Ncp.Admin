using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.OrderLogisticsMethodAggregate;
using Ncp.Admin.Web.Application.Commands.OrderLogisticsMethods;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.OrdersLogisticsMethods;

public record CreateOrderLogisticsMethodRequest(string Name, int TypeValue, int Sort = 0);

public record CreateOrderLogisticsMethodResponse(OrderLogisticsMethodId Id);

public class CreateOrderLogisticsMethodEndpoint(IMediator mediator)
    : Endpoint<CreateOrderLogisticsMethodRequest, ResponseData<CreateOrderLogisticsMethodResponse>>
{
    public override void Configure()
    {
        Tags("OrderLogisticsMethod");
        Description(b => b.AutoTagOverride("OrderLogisticsMethod").WithSummary("创建订单物流方式"));
        Post("/api/admin/order-logistics-methods");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrderCreate);
    }

    public override async Task HandleAsync(CreateOrderLogisticsMethodRequest req, CancellationToken ct)
    {
        var id = await mediator.Send(new CreateOrderLogisticsMethodCommand(
            req.Name, req.TypeValue, req.Sort), ct);
        await Send.OkAsync(new CreateOrderLogisticsMethodResponse(id).AsResponseData(), cancellation: ct);
    }
}
