using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Web.Application.Commands.Customers;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Customer;

/// <summary>
/// 移除客户联系记录请求
/// </summary>
public record RemoveCustomerContactRecordRequest(CustomerId CustomerId, CustomerContactRecordId RecordId);

/// <summary>
/// 移除客户联系记录
/// </summary>
public class RemoveCustomerContactRecordEndpoint(IMediator mediator) : Endpoint<RemoveCustomerContactRecordRequest, ResponseData<bool>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Customer");
        Delete("/api/admin/customers/{customerId}/contact-records/{recordId}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerContactEdit);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(RemoveCustomerContactRecordRequest req, CancellationToken ct)
    {
        await mediator.Send(new RemoveCustomerContactRecordCommand(req.CustomerId, req.RecordId), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
