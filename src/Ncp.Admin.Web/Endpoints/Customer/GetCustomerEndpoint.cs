using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Customer;

public record GetCustomerRequest
{
    public Guid Id { get; set; }
}

public class GetCustomerEndpoint(CustomerQuery query) : Endpoint<GetCustomerRequest, ResponseData<CustomerDetailDto>>
{
    public override void Configure()
    {
        Tags("Customer");
        Get("/api/admin/customers/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerView);
    }

    public override async Task HandleAsync(GetCustomerRequest req, CancellationToken ct)
    {
        var result = await query.GetByIdAsync(new CustomerId(req.Id), ct);
        if (result == null)
        {
            await Send.NotFoundAsync(ct);
            return;
        }
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
