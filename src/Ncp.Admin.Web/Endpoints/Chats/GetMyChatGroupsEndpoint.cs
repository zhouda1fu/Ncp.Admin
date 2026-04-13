using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Chats;

/// <summary>
/// 获取当前用户的聊天组列表
/// </summary>
public class GetMyChatGroupsEndpoint(ChatGroupQuery query)
    : EndpointWithoutRequest<ResponseData<List<ChatGroupQueryDto>>>
{
    public override void Configure()
    {
        Tags("Chat");
        Description(b => b.AutoTagOverride("Chat").WithSummary("获取当前用户的聊天组列表"));
        Get("/api/admin/chat/groups");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ChatView);
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        if (!User.TryGetUserId(out var uid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        var list = await query.GetMyGroupsAsync(uid, ct);
        await Send.OkAsync(list.AsResponseData(), cancellation: ct);
    }
}
