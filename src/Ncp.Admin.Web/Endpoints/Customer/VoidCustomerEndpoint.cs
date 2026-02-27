using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Web.Application.Commands.Customers;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Customer;

/// <summary>
/// 作废客户请求
/// </summary>
/// <param name="Id">要作废的客户 ID（仅公海客户可作废）</param>
public record VoidCustomerRequest(CustomerId Id);

/// <summary>
/// 作废客户
/// </summary>
/// <param name="mediator">MediatR 中介者</param>
public class VoidCustomerEndpoint(IMediator mediator) : Endpoint<VoidCustomerRequest, ResponseData<bool>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Customer");
        Post("/api/admin/customers/{id}/void");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerEdit);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(VoidCustomerRequest req, CancellationToken ct)
    {
        await mediator.Send(new VoidCustomerCommand(req.Id), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
