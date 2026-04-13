using System.Security.Claims;
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
/// 共享客户请求
/// </summary>
/// <param name="Id">客户 ID</param>
/// <param name="SharedToUserIds">共享给用户 ID 列表</param>
public record ShareCustomerRequest(CustomerId Id, IReadOnlyList<UserId> SharedToUserIds);

/// <summary>
/// 共享客户（设置共享人列表）
/// </summary>
/// <param name="mediator">MediatR 中介者</param>
public class ShareCustomerEndpoint(IMediator mediator) : Endpoint<ShareCustomerRequest, ResponseData<bool>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Customer");
        Post("/api/admin/customers/{id}/share");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerShare);
        Description(b => b.AutoTagOverride("Customer").WithSummary("共享客户"));
    }

    /// <inheritdoc />
    public override async Task HandleAsync(ShareCustomerRequest req, CancellationToken ct)
    {
        if (!User.TryGetUserId(out var uid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        await mediator.Send(new ShareCustomerCommand(req.Id, uid, req.SharedToUserIds ?? []), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}

