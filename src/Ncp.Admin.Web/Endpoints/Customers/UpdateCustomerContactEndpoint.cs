using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Web.Application.Commands.Customers;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Customers;

/// <summary>
/// 更新客户联系人请求
/// </summary>
/// <param name="CustomerId">客户 ID</param>
/// <param name="ContactId">联系人 ID</param>
/// <param name="Name">联系人姓名</param>
/// <param name="ContactType">联系类型</param>
/// <param name="Gender">性别</param>
/// <param name="Birthday">生日</param>
/// <param name="Position">职位</param>
/// <param name="Mobile">手机</param>
/// <param name="Phone">电话</param>
/// <param name="Email">邮箱</param>
/// <param name="Qq">QQ</param>
/// <param name="Wechat">微信</param>
/// <param name="IsWechatAdded">微信添加情况</param>
/// <param name="IsPrimary">是否主联系人</param>
public record UpdateCustomerContactRequest(
    CustomerId CustomerId,
    CustomerContactId ContactId,
    string Name,
    string ContactType,
    int Gender,
    DateTimeOffset Birthday,
    string Position,
    string Mobile,
    string Phone,
    string Email,
    string Qq,
    string Wechat,
    bool IsWechatAdded,
    bool IsPrimary);

/// <summary>
/// 更新客户联系人
/// </summary>
/// <param name="mediator">MediatR 中介者</param>
public class UpdateCustomerContactEndpoint(IMediator mediator) : Endpoint<UpdateCustomerContactRequest, ResponseData<bool>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Customer");
        Put("/api/admin/customers/{customerId}/contacts/{contactId}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerContactEdit);
        Description(b => b.AutoTagOverride("Customer").WithSummary("更新客户联系人"));
    }

    /// <inheritdoc />
    public override async Task HandleAsync(UpdateCustomerContactRequest req, CancellationToken ct)
    {
        var cmd = new UpdateCustomerContactCommand(
            req.CustomerId, req.ContactId, req.Name, req.ContactType ?? string.Empty, req.Gender, req.Birthday,
            req.Position ?? string.Empty, req.Mobile ?? string.Empty, req.Phone ?? string.Empty, req.Email ?? string.Empty,
            req.Qq ?? string.Empty, req.Wechat ?? string.Empty, req.IsWechatAdded,
            req.IsPrimary);
        await mediator.Send(cmd, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
