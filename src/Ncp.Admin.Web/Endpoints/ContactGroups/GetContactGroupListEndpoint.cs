using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ContactsGroups;

/// <summary>
/// 获取联系组列表请求
/// </summary>
/// <param name="PageIndex">页码</param>
/// <param name="PageSize">每页条数</param>
/// <param name="Name">名称关键字</param>
public record GetContactGroupListRequest(
    int PageIndex = 1,
    int PageSize = 20,
    string? Name = null);

/// <summary>
/// 获取联系组分页列表
/// </summary>
public class GetContactGroupListEndpoint(ContactGroupQuery query)
    : Endpoint<GetContactGroupListRequest, ResponseData<PagedData<ContactGroupQueryDto>>>
{
    public override void Configure()
    {
        Tags("ContactGroup");
        Description(b => b.AutoTagOverride("ContactGroup").WithSummary("获取联系组分页列表"));
        Get("/api/admin/contact-groups");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ContactGroupView);
    }

    public override async Task HandleAsync(GetContactGroupListRequest req, CancellationToken ct)
    {
        var input = new ContactGroupQueryInput
        {
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            Name = req.Name,
        };
        var result = await query.GetPagedAsync(input, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
