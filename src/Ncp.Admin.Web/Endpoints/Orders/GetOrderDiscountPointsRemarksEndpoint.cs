using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.AppPermissions;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.Services;

namespace Ncp.Admin.Web.Endpoints.Orders;

/// <summary>
/// 获取订单“优惠点数说明”备注列表请求
/// </summary>
public record GetOrderDiscountPointsRemarksRequest(OrderId Id);

/// <summary>
/// 获取订单“优惠点数说明”备注列表
/// </summary>
public class GetOrderDiscountPointsRemarksEndpoint(IMediator mediator) : Endpoint<GetOrderDiscountPointsRemarksRequest, ResponseData<List<OrderRemarkDto>>>
{
    private bool HasPermission(string permissionCode) =>
        User.Claims.Any(c => c.Type == JwtPermissionClaimTypes.Permissions && c.Value == permissionCode);

    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Order");
        Description(b => b.AutoTagOverride("Order").WithSummary("获取订单“优惠点数说明”备注列表"));
        Get("/api/admin/orders/{id}/discount-points-remarks");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        // 访问控制在运行时根据查看/新增权限做差异化处理
        Permissions(PermissionCodes.AllApiAccess);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GetOrderDiscountPointsRemarksRequest req, CancellationToken ct)
    {
        if (!User.TryGetUserId(out var userId))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var hasViewAll = HasPermission(PermissionCodes.OrderDiscountPointsDescriptionView);
        var hasCreateSelf = HasPermission(PermissionCodes.OrderDiscountPointsCreate);
        if (!hasViewAll && !hasCreateSelf)
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }

        var result = await mediator.Send(
            new GetOrderDiscountPointsRemarksQuery(req.Id, userId, CanViewAll: hasViewAll),
            ct);

        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}

