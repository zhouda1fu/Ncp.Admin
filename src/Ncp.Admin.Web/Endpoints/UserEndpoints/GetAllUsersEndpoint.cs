using FastEndpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.UserEndpoints;

/// <summary>
/// 获取所有用户信息的API端点
/// 该端点用于查询系统中的所有用户信息，支持分页、筛选和搜索
/// </summary>
[Tags("Users")]
public class GetAllUsersEndpoint(UserQuery userQuery) : Endpoint<UserQueryInput, ResponseData<PagedData<UserInfoQueryDto>>>
{
  
    public override void Configure()
    {
        Get("/api/users");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.UserView);
    }

   
    public override async Task HandleAsync(UserQueryInput req, CancellationToken ct)
    {
        var result = await userQuery.GetAllUsersAsync(req, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}

