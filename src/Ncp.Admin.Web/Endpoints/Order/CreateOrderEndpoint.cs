using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Orders;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Order;

/// <summary>
/// 创建订单时传入的明细行（所有字段必填）
/// </summary>
public record CreateOrderRequestItemDto(
    ProductId ProductId,
    string ProductName,
    string Model,
    string Number,
    int Qty,
    string Unit,
    decimal Price,
    decimal Amount,
    string Remark);

/// <summary>
/// 创建订单请求
/// </summary>
public record CreateOrderRequest(
    CustomerId CustomerId,
    string CustomerName,
    ProjectId ProjectId,
    ContractId ContractId,
    string OrderNumber,
    OrderType Type,
    OrderStatus Status,
    decimal Amount,
    string Remark,
    UserId OwnerId,
    string OwnerName,
    string ReceiverName,
    string ReceiverPhone,
    string ReceiverAddress,
    DateTimeOffset PayDate,
    DateTimeOffset DeliveryDate,
    IReadOnlyList<CreateOrderRequestItemDto> Items);

/// <summary>
/// 创建订单响应
/// </summary>
/// <param name="Id">新创建的订单 ID</param>
public record CreateOrderResponse(OrderId Id);

/// <summary>
/// 创建订单
/// </summary>
/// <param name="mediator">MediatR 中介者</param>
public class CreateOrderEndpoint(IMediator mediator) : Endpoint<CreateOrderRequest, ResponseData<CreateOrderResponse>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Order");
        Post("/api/admin/orders");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrderCreate);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(CreateOrderRequest req, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out var uid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        var creatorId = new UserId(uid);
        var items = req.Items.Select(x => new CreateOrderItemDto(
            x.ProductId, x.ProductName, x.Model, x.Number, x.Qty, x.Unit, x.Price, x.Amount, x.Remark)).ToList();
        var cmd = new CreateOrderCommand(
            req.CustomerId,
            req.CustomerName,
            req.ProjectId,
            req.ContractId,
            req.OrderNumber,
            req.Type,
            req.Status,
            req.Amount,
            req.Remark ?? string.Empty,
            req.OwnerId,
            req.OwnerName ?? string.Empty,
            req.ReceiverName ?? string.Empty,
            req.ReceiverPhone ?? string.Empty,
            req.ReceiverAddress ?? string.Empty,
            req.PayDate,
            req.DeliveryDate,
            creatorId,
            items);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateOrderResponse(id).AsResponseData(), cancellation: ct);
    }
}
