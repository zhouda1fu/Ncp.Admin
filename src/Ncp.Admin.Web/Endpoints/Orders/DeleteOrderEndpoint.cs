using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Web.Application.Commands.Orders;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Orders;

/// <summary>
/// 删除订单请求
/// </summary>
/// <param name="Id">要删除的订单 ID</param>
public record DeleteOrderRequest(OrderId Id);

/// <summary>
/// 删除订单（软删）
/// </summary>
/// <param name="mediator">MediatR 中介者</param>
public class DeleteOrderEndpoint(IMediator mediator) : Endpoint<DeleteOrderRequest, ResponseData<bool>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Order");
        Description(b => b.AutoTagOverride("Order").WithSummary("删除订单（软删）"));
        Delete("/api/admin/orders/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrderDelete);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(DeleteOrderRequest req, CancellationToken ct)
    {
        await mediator.Send(new DeleteOrderCommand(req.Id), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
