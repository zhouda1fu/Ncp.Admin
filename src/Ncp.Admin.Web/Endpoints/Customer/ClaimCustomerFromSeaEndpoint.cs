using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Web.Application.Commands.Customers;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Customer;

public class ClaimCustomerFromSeaRequest
{
    public Guid Id { get; set; }
    public long? DeptId { get; set; }
}

public class ClaimCustomerFromSeaEndpoint(IMediator mediator) : Endpoint<ClaimCustomerFromSeaRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Customer");
        Post("/api/admin/customers/{id}/claim");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerClaimFromSea);
    }

    public override async Task HandleAsync(ClaimCustomerFromSeaRequest req, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out var uid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        DeptId? deptId = req.DeptId.HasValue ? new DeptId(req.DeptId.Value) : null;
        await mediator.Send(new ClaimCustomerFromSeaCommand(new CustomerId(req.Id), new UserId(uid), deptId), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
