using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Customers;

/// <summary>
/// 客户搜索请求（用于弹窗等场景）
/// </summary>
/// <param name="PageIndex">页码</param>
/// <param name="PageSize">每页条数</param>
/// <param name="Keyword">关键字</param>
/// <param name="OwnerId">负责人</param>
public record GetCustomerSearchRequest(
    int PageIndex = 1,
    int PageSize = 20,
    string? Keyword = null,
    UserId? OwnerId = null);

/// <summary>
/// 客户搜索（分页）
/// </summary>
/// <param name="query">客户查询</param>
public class GetCustomerSearchEndpoint(CustomerQuery query)
    : Endpoint<GetCustomerSearchRequest, ResponseData<PagedData<CustomerSearchDto>>>
{
    /// <inheritdoc />
    public override void Configure()
    {
        Tags("Customer");
        Get("/api/admin/customers/search");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.CustomerView);
        Description(b => b.AutoTagOverride("Customer").WithSummary("客户搜索（分页）"));
    }

    /// <inheritdoc />
    public override async Task HandleAsync(GetCustomerSearchRequest req, CancellationToken ct)
    {
        var input = new CustomerSearchInput
        {
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            Keyword = req.Keyword,
            OwnerId = req.OwnerId,
        };
        var result = await query.SearchAsync(input, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
