using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Identity.Admin.UserEndpoints;

/// <summary>
/// 获取用户信息的请求模型
/// </summary>
/// <param name="Id">要查询的用户ID</param>
public record GetUserRequest(UserId Id);

/// <summary>
/// 获取用户
/// </summary>
/// <param name="userQuery"></param>
public class GetUserEndpoint(UserQuery userQuery) : Endpoint<GetUserRequest, ResponseData<UserInfoQueryDto>>
{
    public override void Configure()
    {
        Tags("Users");
        Description(b => b.AutoTagOverride("Users"));
        Get("/api/admin/users/{id}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.UserView);
    }

    public override async Task HandleAsync(GetUserRequest req, CancellationToken ct)
    {
        var userInfo = await userQuery.GetUserByIdAsync(req.Id, ct);
        if (userInfo == null)
            await Send.NotFoundAsync(ct);
        else
            await Send.OkAsync(userInfo.AsResponseData(), cancellation: ct);
    }
}
