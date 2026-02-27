using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Customers;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Customer;

/// <summary>
/// 从公海领用客户请求
/// </summary>
/// <param name="Id">要领用的客户 ID</param>
public record ClaimCustomerFromSeaRequest(CustomerId Id);

/// <summary>
/// 从公海领用客户（负责人为当前登录用户）
/// </summary>
/// <param name="mediator">MediatR 中介者</param>
public class ClaimCustomerFromSeaEndpoint(IMediator mediator) : Endpoint<ClaimCustomerFromSeaRequest, ResponseData<bool>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Customer");
        Post("/api/admin/customers/{id}/claim");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerClaimFromSea);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(ClaimCustomerFromSeaRequest req, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out var uid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        var ownerName = User.FindFirstValue(ClaimTypes.Name);
        await mediator.Send(new ClaimCustomerFromSeaCommand(req.Id, new UserId(uid), ownerName), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
