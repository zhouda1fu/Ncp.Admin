using System.Security.Claims;
using FastEndpoints;
using FastEndpoints.Swagger;
using MediatR;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.AssetAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Web.Application.Commands.AssetModule;
using Ncp.Admin.Web.AppPermissions;

namespace Ncp.Admin.Web.Endpoints.Asset;

/// <summary>
/// 创建资产请求
/// </summary>
/// <param name="Name">名称</param>
/// <param name="Category">分类</param>
/// <param name="Code">编码</param>
/// <param name="PurchaseDate">购置日期</param>
/// <param name="Value">价值</param>
/// <param name="Remark">备注</param>
public record CreateAssetRequest(string Name, string Category, string Code, DateTimeOffset PurchaseDate, decimal Value, string? Remark);

public class CreateAssetEndpoint(IMediator mediator) : Endpoint<CreateAssetRequest, ResponseData<CreateAssetResponse>>
{
    public override void Configure()
    {
        Tags("Asset");
        Post("/api/admin/assets");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.AssetCreate);
    }

    public override async Task HandleAsync(CreateAssetRequest req, CancellationToken ct)
    {
        var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userIdStr) || !long.TryParse(userIdStr, out var uid))
        {
            await Send.UnauthorizedAsync(ct);
            return;
        }
        var cmd = new CreateAssetCommand(req.Name, req.Category, req.Code, req.PurchaseDate, req.Value, new UserId(uid), req.Remark);
        var id = await mediator.Send(cmd, ct);
        await Send.OkAsync(new CreateAssetResponse(id).AsResponseData(), cancellation: ct);
    }
}

public record CreateAssetResponse(AssetId Id);
