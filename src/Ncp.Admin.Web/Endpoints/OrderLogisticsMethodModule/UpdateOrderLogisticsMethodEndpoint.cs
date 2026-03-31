using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Web.Application.Commands.OrderLogisticsMethodModule;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.OrderLogisticsMethodModule;

public record UpdateOrderLogisticsMethodRequest(string Name, int TypeValue, int Sort = 0);

public class UpdateOrderLogisticsMethodEndpoint(IMediator mediator)
    : Endpoint<UpdateOrderLogisticsMethodRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("OrderLogisticsMethod");
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
