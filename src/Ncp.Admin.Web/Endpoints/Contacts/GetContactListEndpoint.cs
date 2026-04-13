using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Contacts;

/// <summary>
/// 获取联系人列表请求
/// </summary>
/// <param name="PageIndex">页码</param>
/// <param name="PageSize">每页条数</param>
/// <param name="Keyword">姓名/电话/邮箱关键字</param>
/// <param name="GroupId">分组 ID</param>
public record GetContactListRequest(
    int PageIndex = 1,
    int PageSize = 20,
    string? Keyword = null,
    Guid? GroupId = null);

/// <summary>
/// 获取联系人分页列表
/// </summary>
public class GetContactListEndpoint(ContactQuery query)
    : Endpoint<GetContactListRequest, ResponseData<PagedData<ContactQueryDto>>>
{
    public override void Configure()
    {
        Tags("Contact");
        Description(b => b.AutoTagOverride("Contact").WithSummary("获取联系人分页列表"));
        Get("/api/admin/contacts");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ContactView);
    }

    public override async Task HandleAsync(GetContactListRequest req, CancellationToken ct)
    {
        var input = new ContactQueryInput
        {
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            Keyword = req.Keyword,
            GroupId = req.GroupId,
        };
        var result = await query.GetPagedAsync(input, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
