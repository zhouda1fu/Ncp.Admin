using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Web.Application.Commands.Customers;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Customer;

/// <summary>
/// 移除客户联系人请求
/// </summary>
/// <param name="CustomerId">客户 ID</param>
/// <param name="ContactId">联系人 ID</param>
public record RemoveCustomerContactRequest(CustomerId CustomerId, CustomerContactId ContactId);

/// <summary>
/// 移除客户联系人
/// </summary>
/// <param name="mediator">MediatR 中介者</param>
public class RemoveCustomerContactEndpoint(IMediator mediator) : Endpoint<RemoveCustomerContactRequest, ResponseData<bool>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Customer");
        Delete("/api/admin/customers/{customerId}/contacts/{contactId}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerContactEdit);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(RemoveCustomerContactRequest req, CancellationToken ct)
    {
        await mediator.Send(new RemoveCustomerContactCommand(req.CustomerId, req.ContactId), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
