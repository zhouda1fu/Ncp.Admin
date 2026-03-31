using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Web.Application.Commands.OrderLogisticsCompanyModule;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.OrderLogisticsCompanyModule;

public class DeleteOrderLogisticsCompanyEndpoint(IMediator mediator)
    : EndpointWithoutRequest<ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("OrderLogisticsCompany");
        Delete("/api/admin/order-logistics-companies/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrderDelete);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var id = new OrderLogisticsCompanyId(Route<Guid>("id"));
        await mediator.Send(new DeleteOrderLogisticsCompanyCommand(id), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
