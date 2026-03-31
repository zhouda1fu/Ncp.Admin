using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Web.Application.Commands.OrderLogisticsCompanyModule;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.OrderLogisticsCompanyModule;

public record CreateOrderLogisticsCompanyRequest(string Name, int TypeValue = 0, int Sort = 0);

public record CreateOrderLogisticsCompanyResponse(OrderLogisticsCompanyId Id);

public class CreateOrderLogisticsCompanyEndpoint(IMediator mediator)
    : Endpoint<CreateOrderLogisticsCompanyRequest, ResponseData<CreateOrderLogisticsCompanyResponse>>
{
    public override void Configure()
    {
        Tags("OrderLogisticsCompany");
        Post("/api/admin/order-logistics-companies");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrderCreate);
    }

    public override async Task HandleAsync(CreateOrderLogisticsCompanyRequest req, CancellationToken ct)
    {
        var id = await mediator.Send(new CreateOrderLogisticsCompanyCommand(
            req.Name, req.TypeValue, req.Sort), ct);
        await Send.OkAsync(new CreateOrderLogisticsCompanyResponse(id).AsResponseData(), cancellation: ct);
    }
}
