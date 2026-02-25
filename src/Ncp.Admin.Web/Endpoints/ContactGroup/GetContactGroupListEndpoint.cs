using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ContactGroup;

/// <summary>
/// 获取联系组列表请求
/// </summary>
public class GetContactGroupListRequest : ContactGroupQueryInput { }

/// <summary>
/// 获取联系组分页列表
/// </summary>
public class GetContactGroupListEndpoint(ContactGroupQuery query)
    : Endpoint<GetContactGroupListRequest, ResponseData<PagedData<ContactGroupQueryDto>>>
{
    public override void Configure()
    {
        Tags("ContactGroup");
        Get("/api/admin/contact-groups");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ContactGroupView);
    }

    public override async Task HandleAsync(GetContactGroupListRequest req, CancellationToken ct)
    {
        var result = await query.GetPagedAsync(req, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
