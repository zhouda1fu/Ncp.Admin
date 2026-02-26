using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Web.Application.Commands.Customers;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Customer;

public class ReleaseCustomerToSeaRequest
{
    public Guid Id { get; set; }
}

public class ReleaseCustomerToSeaEndpoint(IMediator mediator) : Endpoint<ReleaseCustomerToSeaRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Customer");
        Post("/api/admin/customers/{id}/release-to-sea");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerReleaseToSea);
    }

    public override async Task HandleAsync(ReleaseCustomerToSeaRequest req, CancellationToken ct)
    {
        await mediator.Send(new ReleaseCustomerToSeaCommand(new CustomerId(req.Id)), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
