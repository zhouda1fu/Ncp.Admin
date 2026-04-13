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
/// 删除订单“优惠点数说明”备注请求
/// </summary>
public record DeleteOrderDiscountPointsRemarkRequest(OrderId Id, OrderRemarkId RemarkId);

/// <summary>
/// 删除订单“优惠点数说明”备注
/// </summary>
public class DeleteOrderDiscountPointsRemarkEndpoint(IMediator mediator) : Endpoint<DeleteOrderDiscountPointsRemarkRequest, ResponseData<bool>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Order");
        Description(b => b.AutoTagOverride("Order").WithSummary("删除订单“优惠点数说明”备注"));
        Delete("/api/admin/orders/{id}/discount-points-remarks/{remarkId}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.OrderDiscountPointsCreate);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(DeleteOrderDiscountPointsRemarkRequest req, CancellationToken ct)
    {
        var userId = User.GetUserIdOrNull() ?? new UserId(0);

        await mediator.Send(new DeleteOrderDiscountPointsRemarkCommand(req.Id, req.RemarkId, userId), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}

