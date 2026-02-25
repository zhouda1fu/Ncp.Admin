using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Contact;

/// <summary>
/// 获取联系人列表请求
/// </summary>
public class GetContactListRequest : ContactQueryInput { }

/// <summary>
/// 获取联系人分页列表
/// </summary>
public class GetContactListEndpoint(ContactQuery query)
    : Endpoint<GetContactListRequest, ResponseData<PagedData<ContactQueryDto>>>
{
    public override void Configure()
    {
        Tags("Contact");
        Get("/api/admin/contacts");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ContactView);
    }

    public override async Task HandleAsync(GetContactListRequest req, CancellationToken ct)
    {
        var result = await query.GetPagedAsync(req, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
