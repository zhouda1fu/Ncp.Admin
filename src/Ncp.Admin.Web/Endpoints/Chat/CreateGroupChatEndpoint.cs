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
/// 创建群聊请求
/// </summary>
public class CreateGroupChatRequest
{
    public string Name { get; set; } = "";
    public List<long> MemberIds { get; set; } = [];
}

/// <summary>
/// 创建群聊
/// </summary>
public class CreateGroupChatEndpoint(IMediator mediator)
    : Endpoint<CreateGroupChatRequest, ResponseData<CreateChatGroupResponse>>
{
    public override void Configure()
    {
        Tags("Chat");
        Post("/api/admin/chat/groups/group");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.ChatCreate);
    }

    public override async Task HandleAsync(CreateGroupChatRequest req, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out var uid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        var memberIds = (req.MemberIds ?? []).Select(x => new UserId(x)).ToList();
        var cmd = new CreateGroupChatCommand(new UserId(uid), req.Name, memberIds);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateChatGroupResponse(id).AsResponseData(), cancellation: ct);
    }
}
