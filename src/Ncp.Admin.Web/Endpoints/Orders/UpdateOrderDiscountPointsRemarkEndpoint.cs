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
/// 更新订单“优惠点数说明”备注请求
/// </summary>
public record UpdateOrderDiscountPointsRemarkRequest(OrderId Id, OrderRemarkId RemarkId, string Content);

/// <summary>
/// 更新订单“优惠点数说明”备注
/// </summary>
public class UpdateOrderDiscountPointsRemarkEndpoint(IMediator mediator) : Endpoint<UpdateOrderDiscountPointsRemarkRequest, ResponseData<bool>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Order");
        Description(b => b.AutoTagOverride("Order").WithSummary("更新订单“优惠点数说明”备注"));
        Put("/api/admin/orders/{id}/discount-points-remarks/{remarkId}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrderDiscountPointsCreate);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(UpdateOrderDiscountPointsRemarkRequest req, CancellationToken ct)
    {
        var userId = User.GetUserIdOrNull() ?? new UserId(0);

        await mediator.Send(new UpdateOrderDiscountPointsRemarkCommand(req.Id, req.RemarkId, userId, req.Content ?? string.Empty), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}

