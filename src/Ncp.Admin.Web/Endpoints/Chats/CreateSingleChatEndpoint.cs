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
/// 创建单聊请求
/// </summary>
/// <param name="OtherUserId">对方用户 ID</param>
public record CreateSingleChatRequest(UserId OtherUserId);

/// <summary>
/// 创建单聊（与指定用户 1:1）
/// </summary>
public class CreateSingleChatEndpoint(IMediator mediator)
    : Endpoint<CreateSingleChatRequest, ResponseData<CreateChatGroupResponse>>
{
    public override void Configure()
    {
        Tags("Chat");
        Description(b => b.AutoTagOverride("Chat").WithSummary("创建单聊（与指定用户 1:1）"));
        Post("/api/admin/chat/groups/single");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ChatCreate);
    }

    public override async Task HandleAsync(CreateSingleChatRequest req, CancellationToken ct)
    {
        if (!User.TryGetUserId(out var uid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        var cmd = new CreateSingleChatCommand(uid, req.OtherUserId);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateChatGroupResponse(id).AsResponseData(), cancellation: ct);
    }
}

public record CreateChatGroupResponse(ChatGroupId Id);
