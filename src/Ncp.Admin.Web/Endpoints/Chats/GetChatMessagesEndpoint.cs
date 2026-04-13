using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Chats;

/// <summary>
/// 获取聊天组消息列表（分页）
/// </summary>
/// <param name="ChatGroupId">聊天组 ID（可与路由参数绑定）</param>
/// <param name="PageIndex">页码</param>
/// <param name="PageSize">每页条数</param>
public record GetChatMessagesRequest(
    Guid ChatGroupId,
    int PageIndex = 1,
    int PageSize = 20);

/// <summary>
/// 获取某聊天组的消息分页列表
/// </summary>
public class GetChatMessagesEndpoint(ChatMessageQuery query)
    : Endpoint<GetChatMessagesRequest, ResponseData<PagedData<ChatMessageQueryDto>>>
{
    public override void Configure()
    {
        Tags("Chat");
        Description(b => b.AutoTagOverride("Chat").WithSummary("获取某聊天组的消息分页列表"));
        Get("/api/admin/chat/groups/{chatGroupId}/messages");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ChatView);
    }

    public override async Task HandleAsync(GetChatMessagesRequest req, CancellationToken ct)
    {
        var input = new ChatMessageQueryInput
        {
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            ChatGroupId = req.ChatGroupId,
        };
        var result = await query.GetPagedByGroupIdAsync(input, ct);
        await Send.OkAsync(result.AsResponseData(), cancellation: ct);
    }
}
