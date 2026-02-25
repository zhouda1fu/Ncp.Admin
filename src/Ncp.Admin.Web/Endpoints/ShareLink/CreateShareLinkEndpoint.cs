using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.DocumentAggregate;
using Ncp.Admin.Domain.AggregatesModel.ShareLinkAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.ShareLink;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ShareLink;

/// <summary>
/// 创建共享链接请求
/// </summary>
public class CreateShareLinkRequest
{
    public Guid DocumentId { get; set; }
    public DateTimeOffset? ExpiresAt { get; set; }
}

/// <summary>
/// 创建文档共享链接
/// </summary>
public class CreateShareLinkEndpoint(IMediator mediator)
    : Endpoint<CreateShareLinkRequest, ResponseData<CreateShareLinkResponse>>
{
    public override void Configure()
    {
        Tags("ShareLink");
        Post("/api/admin/share-links");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.DocumentShare);
    }

    public override async Task HandleAsync(CreateShareLinkRequest req, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out var uid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        var cmd = new CreateShareLinkCommand(new DocumentId(req.DocumentId), new UserId(uid), req.ExpiresAt);
        var (id, token) = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateShareLinkResponse(id, token).AsResponseData(), cancellation: ct);
    }
}

public record CreateShareLinkResponse(ShareLinkId Id, string Token);
