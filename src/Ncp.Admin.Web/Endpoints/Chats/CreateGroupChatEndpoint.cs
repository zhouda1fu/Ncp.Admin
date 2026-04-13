using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ChatGroupAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.ChatGroups;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Chats;

/// <summary>
/// 创建群聊请求
/// </summary>
/// <param name="Name">群组名称</param>
/// <param name="MemberIds">成员用户 ID 列表</param>
public record CreateGroupChatRequest(string Name, IReadOnlyList<UserId> MemberIds);

/// <summary>
/// 创建群聊
/// </summary>
public class CreateGroupChatEndpoint(IMediator mediator)
    : Endpoint<CreateGroupChatRequest, ResponseData<CreateChatGroupResponse>>
{
    public override void Configure()
    {
        Tags("Chat");
        Description(b => b.AutoTagOverride("Chat").WithSummary("创建群聊"));
        Post("/api/admin/chat/groups/group");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ChatCreate);
    }

    public override async Task HandleAsync(CreateGroupChatRequest req, CancellationToken ct)
    {
        if (!User.TryGetUserId(out var uid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        var cmd = new CreateGroupChatCommand(uid, req.Name, req.MemberIds ?? Array.Empty<UserId>());
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateChatGroupResponse(id).AsResponseData(), cancellation: ct);
    }
}
