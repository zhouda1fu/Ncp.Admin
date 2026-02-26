using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Web.Application.Commands.Customers;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Customer;

public class RemoveCustomerContactRequest
{
    public Guid CustomerId { get; set; }
    public Guid ContactId { get; set; }
}

public class RemoveCustomerContactEndpoint(IMediator mediator) : Endpoint<RemoveCustomerContactRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Customer");
        Delete("/api/admin/customers/{customerId}/contacts/{contactId}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerContactEdit);
    }

    public override async Task HandleAsync(RemoveCustomerContactRequest req, CancellationToken ct)
    {
        await mediator.Send(new RemoveCustomerContactCommand(new CustomerId(req.CustomerId), new CustomerContactId(req.ContactId)), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
