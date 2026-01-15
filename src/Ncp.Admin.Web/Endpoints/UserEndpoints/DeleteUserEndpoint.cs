using FastEndpoints;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.UserCommands;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.UserEndpoints;

/// <summary>
/// 删除用户的请求模型
/// </summary>
/// <param name="UserId">要删除的用户ID</param>
public record DeleteUserRequest(UserId UserId);

/// <summary>
/// 删除用户的API端点
/// 该端点用于从系统中删除指定的用户账户（软删除）
/// </summary>
[Tags("Users")]
public class DeleteUserEndpoint(IMediator mediator) : Endpoint<DeleteUserRequest, ResponseData<bool>>
{
    public override void Configure()
    {
        Delete("/api/users/{userId}");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.UserDelete);
    }

    public override async Task HandleAsync(DeleteUserRequest request, CancellationToken ct)
    {
        var command = new DeleteUserCommand(request.UserId);
        await mediator.Send(command, ct);
        await Send.OkAsync(true.AsResponseData(), cancellation: ct);
    }
}

