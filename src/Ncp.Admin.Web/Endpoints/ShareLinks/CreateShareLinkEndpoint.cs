using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.DocumentAggregate;
using Ncp.Admin.Domain.AggregatesModel.ShareLinkAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.ShareLinks;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.ShareLinks;

/// <summary>
/// 创建共享链接请求
/// </summary>
/// <param name="DocumentId">文档 ID</param>
/// <param name="ExpiresAt">过期时间，null 表示永久</param>
public record CreateShareLinkRequest(DocumentId DocumentId, DateTimeOffset? ExpiresAt);

/// <summary>
/// 创建文档共享链接
/// </summary>
public class CreateShareLinkEndpoint(IMediator mediator)
    : Endpoint<CreateShareLinkRequest, ResponseData<CreateShareLinkResponse>>
{
    public override void Configure()
    {
        Tags("ShareLink");
        Description(b => b.AutoTagOverride("ShareLink").WithSummary("创建文档共享链接"));
        Post("/api/admin/share-links");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.DocumentShare);
    }

    public override async Task HandleAsync(CreateShareLinkRequest req, CancellationToken ct)
    {
        if (!User.TryGetUserId(out var uid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        var cmd = new CreateShareLinkCommand(req.DocumentId, uid, req.ExpiresAt);
        var (id, token) = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateShareLinkResponse(id, token).AsResponseData(), cancellation: ct);
    }
}

public record CreateShareLinkResponse(ShareLinkId Id, string Token);
