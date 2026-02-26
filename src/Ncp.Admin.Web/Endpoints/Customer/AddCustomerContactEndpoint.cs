using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Web.Application.Commands.Customers;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Customer;

public class AddCustomerContactRequest
{
    public Guid CustomerId { get; set; }
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

public record AddCustomerContactResponse(CustomerContactId Id);

public class AddCustomerContactEndpoint(IMediator mediator) : Endpoint<AddCustomerContactRequest, ResponseData<AddCustomerContactResponse>>
{
    public override void Configure()
    {
        Tags("Customer");
        Post("/api/admin/customers/{customerId}/contacts");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerContactEdit);
    }

    public override async Task HandleAsync(AddCustomerContactRequest req, CancellationToken ct)
    {
        var cmd = new AddCustomerContactCommand(
            new CustomerId(req.CustomerId), req.Name, req.ContactType, req.Gender, req.Birthday,
            req.Position, req.Mobile, req.Phone, req.Email, req.IsPrimary);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new AddCustomerContactResponse(id).AsResponseData(), cancellation: ct);
    }
}
