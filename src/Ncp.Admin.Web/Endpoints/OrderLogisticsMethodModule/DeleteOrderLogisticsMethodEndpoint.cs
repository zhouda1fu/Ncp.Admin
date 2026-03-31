using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Web.Application.Commands.OrderLogisticsMethodModule;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.OrderLogisticsMethodModule;

public class DeleteOrderLogisticsMethodEndpoint(IMediator mediator)
    : EndpointWithoutRequest<ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("OrderLogisticsMethod");
        Delete("/api/admin/order-logistics-methods/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrderDelete);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = new OrderLogisticsMethodId(Route<Guid>("id"));
        await mediator.Send(new DeleteOrderLogisticsMethodCommand(id), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
