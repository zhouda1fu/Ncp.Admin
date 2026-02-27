using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Web.Application.Commands.Customers;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Customer;

/// <summary>
/// 删除客户请求
/// </summary>
/// <param name="Id">要删除的客户 ID</param>
public record DeleteCustomerRequest(CustomerId Id);

/// <summary>
/// 删除客户
/// </summary>
/// <param name="mediator">MediatR 中介者</param>
public class DeleteCustomerEndpoint(IMediator mediator) : Endpoint<DeleteCustomerRequest, ResponseData<bool>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Customer");
        Delete("/api/admin/customers/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerDelete);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(DeleteCustomerRequest req, CancellationToken ct)
    {
        await mediator.Send(new DeleteCustomerCommand(req.Id), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
