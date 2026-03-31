using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Web.Application.Commands.OrderLogisticsCompanyModule;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.OrderLogisticsCompanyModule;

public record UpdateOrderLogisticsCompanyRequest(string Name, int TypeValue = 0, int Sort = 0);

public class UpdateOrderLogisticsCompanyEndpoint(IMediator mediator)
    : Endpoint<UpdateOrderLogisticsCompanyRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("OrderLogisticsCompany");
        Put("/api/admin/order-logistics-companies/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrderEdit);
    }

    public override async Task HandleAsync(UpdateOrderLogisticsCompanyRequest req, CancellationToken ct)
    {
        var id = new OrderLogisticsCompanyId(Route<Guid>("id"));
        await mediator.Send(new UpdateOrderLogisticsCompanyCommand(
            id, req.Name, req.TypeValue, req.Sort), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
