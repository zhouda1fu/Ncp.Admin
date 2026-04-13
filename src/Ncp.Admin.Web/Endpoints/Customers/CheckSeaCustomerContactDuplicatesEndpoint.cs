using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.AppPermissions;
using Ncp.Admin.Web.Application.Queries.Customers;

namespace Ncp.Admin.Web.Endpoints.Customers;

/// <summary>
/// 公海录入：检查联系方式（电话/QQ/微信）是否与客户表重复
/// </summary>
/// <param name="MainContactPhone">主联系人电话</param>
/// <param name="ContactQq">QQ</param>
/// <param name="ContactWechat">微信</param>
public record CheckSeaCustomerContactDuplicatesRequest(
    string MainContactPhone,
    string ContactQq,
    string ContactWechat);

/// <summary>
/// 公海录入：联系方式重复校验响应
/// </summary>
/// <param name="Items">命中的重复项（展示全部）</param>
public record CheckSeaCustomerContactDuplicatesResponse(IReadOnlyList<SeaCustomerDuplicateContactItem> Items);

/// <summary>
/// 公海录入：联系方式重复校验接口
/// </summary>
/// <param name="mediator">MediatR 中介者</param>
public class CheckSeaCustomerContactDuplicatesEndpoint(IMediator mediator)
    : Endpoint<CheckSeaCustomerContactDuplicatesRequest, ResponseData<CheckSeaCustomerContactDuplicatesResponse>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Customer");
        Post("/api/admin/customers/sea/check-duplicate-contacts");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerCreate);
        Description(b => b.AutoTagOverride("Customer").WithSummary("公海录入：检查联系方式是否重复"));
    }

    /// <inheritdoc />
    public override async Task HandleAsync(CheckSeaCustomerContactDuplicatesRequest req, CancellationToken ct)
    {
        var items = await mediator.Send(
            new CheckSeaCustomerContactDuplicatesQuery(req.MainContactPhone, req.ContactQq, req.ContactWechat),
            ct);

        await Send.OkAsync(new CheckSeaCustomerContactDuplicatesResponse(items).AsResponseData(), ct);
    }
}

