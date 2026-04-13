using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.OrderLogisticsCompanyAggregate;
using Ncp.Admin.Web.Application.Commands.OrderLogisticsCompanies;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.OrdersLogisticsCompanies;

public class DeleteOrderLogisticsCompanyEndpoint(IMediator mediator)
    : EndpointWithoutRequest<ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("OrderLogisticsCompany");
        Description(b => b.AutoTagOverride("OrderLogisticsCompany").WithSummary("删除订单物流公司"));
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
