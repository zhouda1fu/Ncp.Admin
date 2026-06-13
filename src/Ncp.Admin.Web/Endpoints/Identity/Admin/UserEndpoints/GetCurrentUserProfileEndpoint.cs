using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;
using Ncp.Admin.Web.Extensions;

namespace Ncp.Admin.Web.Endpoints.Identity.Admin.UserEndpoints;

/// <summary>
/// 获取当前登录用户资料。
/// </summary>
public class GetCurrentUserProfileEndpoint(UserQuery userQuery) : EndpointWithoutRequest<ResponseData<UserProfileResponse>>
{
    public override void Configure()
    {
        Tags("Users");
        Description(b => b.AutoTagOverride("Users"));
        Get("/api/admin/user/profile");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var currentUserId = User.GetUserIdOrNull() ?? throw new KnownException("无效的用户身份", ErrorCodes.InvalidUser);
        var userInfo = await userQuery.GetUserByIdAsync(currentUserId, ct);
        var response = new UserProfileResponse(
            userInfo.UserId,
            userInfo.Name,
            userInfo.Phone,
            userInfo.Roles,
            userInfo.RealName,
            userInfo.Status,
            userInfo.Email,
            userInfo.CreatedAt,
            userInfo.Gender,
            userInfo.Age,
            userInfo.BirthDate,
            userInfo.DeptId,
            userInfo.DeptName,
            userInfo.AvatarUrl
        );
        await Send.OkAsync(response.AsResponseData(), cancellation: ct);
    }
}
