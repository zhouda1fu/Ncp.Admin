using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Identity.Admin.UserEndpoints;

/// <summary>
/// 获取用户列表
/// </summary>
/// <param name="userQuery"></param>
public class GetAllUsersEndpoint(UserQuery userQuery) : Endpoint<UserQueryInput, ResponseData<PagedData<UserInfoQueryDto>>>
{
    public override void Configure()
    {
        Tags("Users");
        Description(b => b.AutoTagOverride("Users"));
        Get("/api/admin/users");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.UserView);
    }

    public override async Task HandleAsync(UserQueryInput req, CancellationToken ct)
    {
        var result = await userQuery.GetAllUsersAsync(req, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
