using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Customers;
using Ncp.Admin.Web.AppPermissions;
using NetCorePal.Extensions.Primitives;

namespace Ncp.Admin.Web.Endpoints.Customers;

/// <summary>
/// 移除客户联系记录请求
/// </summary>
public record RemoveCustomerContactRecordRequest(CustomerId CustomerId, CustomerContactRecordId RecordId);

/// <summary>
/// 移除客户联系记录
/// </summary>
public class RemoveCustomerContactRecordEndpoint(IMediator mediator) : Endpoint<RemoveCustomerContactRecordRequest, ResponseData<bool>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Customer");
        Delete("/api/admin/customers/{customerId}/contact-records/{recordId}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerContactEdit);
        Description(b => b.AutoTagOverride("Customer").WithSummary("移除客户联系记录"));
    }

    /// <inheritdoc />
    public override async Task HandleAsync(RemoveCustomerContactRecordRequest req, CancellationToken ct)
    {
        if (!User.TryGetUserId(out var removerId))
            throw new KnownException("未登录或用户标识无效");
        await mediator.Send(new RemoveCustomerContactRecordCommand(req.CustomerId, req.RecordId, removerId), ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
