using System.Security.Claims;
using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.Customers;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Customer;

/// <summary>
/// 添加客户联系记录请求
/// </summary>
public record AddCustomerContactRecordRequest(
    CustomerId CustomerId,
    DateTimeOffset RecordAt,
    string RecordType,
    string Content);

/// <summary>
/// 添加客户联系记录响应
/// </summary>
public record AddCustomerContactRecordResponse(CustomerContactRecordId Id);

/// <summary>
/// 添加客户联系记录
/// </summary>
public class AddCustomerContactRecordEndpoint(IMediator mediator) : Endpoint<AddCustomerContactRecordRequest, ResponseData<AddCustomerContactRecordResponse>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Customer");
        Post("/api/admin/customers/{customerId}/contact-records");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerContactEdit);
    }

    /// <inheritdoc />
    public override async Task HandleAsync(AddCustomerContactRecordRequest req, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        long? uid = null;
        if (!string.IsNullOrEmpty(userIdStr) && long.TryParse(userIdStr, out var parsed))
            uid = parsed;
        var recorderName = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
        var recorderId = uid.HasValue ? new UserId(uid.Value) : (UserId?)null;
        var cmd = new AddCustomerContactRecordCommand(
            req.CustomerId, req.RecordAt, req.RecordType ?? string.Empty, req.Content ?? string.Empty, recorderId, recorderName);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new AddCustomerContactRecordResponse(id).AsResponseData(), cancellation: ct);
    }
}
