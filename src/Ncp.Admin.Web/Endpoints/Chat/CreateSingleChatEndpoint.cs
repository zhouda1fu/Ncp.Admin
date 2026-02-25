using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.ChatGroupAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.ChatGroup;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Chat;

/// <summary>
/// 创建单聊请求
/// </summary>
public class CreateSingleChatRequest
{
    public long OtherUserId { get; set; }
}

/// <summary>
/// 创建单聊（与指定用户 1:1）
/// </summary>
public class CreateSingleChatEndpoint(IMediator mediator)
    : Endpoint<CreateSingleChatRequest, ResponseData<CreateChatGroupResponse>>
{
    public override void Configure()
    {
        Tags("Chat");
        Post("/api/admin/chat/groups/single");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ChatCreate);
    }

    public override async Task HandleAsync(CreateSingleChatRequest req, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out var uid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        var cmd = new CreateSingleChatCommand(new UserId(uid), new UserId(req.OtherUserId));
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateChatGroupResponse(id).AsResponseData(), cancellation: ct);
    }
}

public record CreateChatGroupResponse(ChatGroupId Id);
