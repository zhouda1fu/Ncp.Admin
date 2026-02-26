using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Web.Application.Commands.Customers;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Customer;

public class UpdateCustomerContactRequest
{
    public Guid CustomerId { get; set; }
    public Guid ContactId { get; set; }
    public string Name { get; set; } = "";
    public string? ContactType { get; set; }
    public int? Gender { get; set; }
    public DateTime? Birthday { get; set; }
    public string? Position { get; set; }
    public string? Mobile { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public bool IsPrimary { get; set; }
}

public class UpdateCustomerContactEndpoint(IMediator mediator) : Endpoint<UpdateCustomerContactRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Customer");
        Put("/api/admin/customers/{customerId}/contacts/{contactId}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerContactEdit);
    }

    public override async Task HandleAsync(UpdateCustomerContactRequest req, CancellationToken ct)
    {
        var cmd = new UpdateCustomerContactCommand(
            new CustomerId(req.CustomerId), new CustomerContactId(req.ContactId), req.Name, req.ContactType, req.Gender, req.Birthday,
            req.Position, req.Mobile, req.Phone, req.Email, req.IsPrimary);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
