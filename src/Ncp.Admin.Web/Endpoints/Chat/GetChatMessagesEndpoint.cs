using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Chat;

/// <summary>
/// 获取聊天组消息列表（分页）
/// </summary>
public class GetChatMessagesRequest : ChatMessageQueryInput { }

/// <summary>
/// 获取某聊天组的消息分页列表
/// </summary>
public class GetChatMessagesEndpoint(ChatMessageQuery query)
    : Endpoint<GetChatMessagesRequest, ResponseData<PagedData<ChatMessageQueryDto>>>
{
    public override void Configure()
    {
        Tags("Chat");
        Get("/api/admin/chat/groups/{chatGroupId}/messages");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ChatView);
    }

    public override async Task HandleAsync(GetChatMessagesRequest req, CancellationToken ct)
    {
        var result = await query.GetPagedByGroupIdAsync(req, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
