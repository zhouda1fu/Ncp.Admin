using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Chat;

/// <summary>
/// 获取当前用户的聊天组列表
/// </summary>
public class GetMyChatGroupsEndpoint(ChatGroupQuery query)
    : EndpointWithoutRequest<ResponseData<List<ChatGroupQueryDto>>>
{
    public override void Configure()
    {
        Tags("Chat");
        Get("/api/admin/chat/groups");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ChatView);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out var uid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        var list = await query.GetMyGroupsAsync(new UserId(uid), ct);
        await Send.OkAsync(list.AsResponseData(), cancellation: ct);
    }
}
