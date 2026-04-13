using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Web.Application.Commands.Customers;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Customers;

/// <summary>
/// 释放客户到公海请求
/// </summary>
/// <param name="Id">要释放的客户 ID</param>
public record ReleaseCustomerToSeaRequest(CustomerId Id);

/// <summary>
/// 释放客户到公海
/// </summary>
/// <param name="mediator">MediatR 中介者</param>
public class ReleaseCustomerToSeaEndpoint(IMediator mediator) : Endpoint<ReleaseCustomerToSeaRequest, ResponseData<bool>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Customer");
        Post("/api/admin/customers/{id}/release-to-sea");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerReleaseToSea);
        Description(b => b.AutoTagOverride("Customer").WithSummary("释放客户到公海"));
    }

    /// <inheritdoc />
    public override async Task HandleAsync(ReleaseCustomerToSeaRequest req, CancellationToken ct)
    {
        await mediator.Send(new ReleaseCustomerToSeaCommand(req.Id), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
