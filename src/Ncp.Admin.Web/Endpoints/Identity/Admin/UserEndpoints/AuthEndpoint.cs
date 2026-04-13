using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using NetCorePal.Extensions.Dto;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Identity.Admin.UserEndpoints;

/// <summary>
/// 认证校验
/// </summary>
public class AuthEndpoint : EndpointWithoutRequest<ResponseData<bool>>
{
    public override void Configure()
    {
        Tags("Users");
        Description(b => b.AutoTagOverride("Users").WithSummary("认证校验"));
        Get("/api/admin/user/auth");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.UserView);
    }

    public override Task HandleAsync(CancellationToken ct)
    {
        return Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}
