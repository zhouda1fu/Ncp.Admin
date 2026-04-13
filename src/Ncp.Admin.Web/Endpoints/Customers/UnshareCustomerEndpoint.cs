using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Customers;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Customers;

/// <summary>
/// 取消共享客户请求
/// </summary>
/// <param name="Id">客户 ID</param>
/// <param name="SharedToUserIds">取消共享的用户 ID 列表</param>
public record UnshareCustomerRequest(CustomerId Id, IReadOnlyList<UserId> SharedToUserIds);

/// <summary>
/// 取消共享客户（从共享列表中移除指定用户）
/// </summary>
/// <param name="mediator">MediatR 中介者</param>
public class UnshareCustomerEndpoint(IMediator mediator) : Endpoint<UnshareCustomerRequest, ResponseData<bool>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Customer");
        Delete("/api/admin/customers/{id}/share");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerShare);
        Description(b => b.AutoTagOverride("Customer").WithSummary("取消共享客户"));
    }

    /// <inheritdoc />
    public override async Task HandleAsync(UnshareCustomerRequest req, CancellationToken ct)
    {
        await mediator.Send(new UnshareCustomerCommand(req.Id, req.SharedToUserIds ?? []), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}

