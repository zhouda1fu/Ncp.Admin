using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Orders;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Orders;

/// <summary>
/// 创建订单“优惠点数说明”备注
/// </summary>
public record CreateOrderDiscountPointsRemarkRequest(OrderId Id, string Content);

/// <summary>
/// 创建订单“优惠点数说明”备注
/// </summary>
public class CreateOrderDiscountPointsRemarkEndpoint(IMediator mediator) : Endpoint<CreateOrderDiscountPointsRemarkRequest, ResponseData<bool>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Order");
        Description(b => b.AutoTagOverride("Order").WithSummary("创建订单“优惠点数说明”备注"));
        Post("/api/admin/orders/{id}/discount-points-remarks");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrderDiscountPointsCreate);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(CreateOrderDiscountPointsRemarkRequest req, CancellationToken ct)
    {
        var userId = User.GetUserIdOrNull() ?? new UserId(0);

        await mediator.Send(new CreateOrderDiscountPointsRemarkCommand(req.Id, userId, req.Content ?? string.Empty), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}

