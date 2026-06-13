using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Ncp.Admin.Domain.AggregatesModel.RoleAggregate;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;
using NetCorePal.Extensions.Dto;

namespace Ncp.Admin.Web.Endpoints.Identity.Admin.RoleEndpoints;

/// <summary>角色字段级修改历史分页（由操作日志请求体对比生成）。</summary>
public record GetRoleChangeHistoryRequest(
    RoleId RoleId,
    int PageIndex = 1,
    int PageSize = 20,
    string? Keyword = null);

/// <summary>
/// 角色字段级修改历史。
/// </summary>
public class GetRoleChangeHistoryEndpoint(RoleChangeHistoryQuery historyQuery)
    : Endpoint<GetRoleChangeHistoryRequest, ResponseData<PagedData<RoleFieldChangeRowDto>>>
{
    public override void Configure()
    {
        Tags("Roles");
        Description(b => b.AutoTagOverride("Roles").WithSummary("角色字段级修改历史"));
        Get("/api/admin/roles/{roleId:guid}/change-history");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.RoleView);
    }

    public override async Task HandleAsync(GetRoleChangeHistoryRequest req, CancellationToken ct)
    {
        var exists = await historyQuery.ExistsAsync(req.RoleId, ct);
        if (!exists)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var input = new PageRequest
        {
            PageIndex = req.PageIndex,
            PageSize = req.PageSize,
            CountTotal = true,
        };
        var data = await historyQuery.GetFieldChangeHistoryPagedAsync(req.RoleId, input, req.Keyword, ct);
        await Send.OkAsync(data.AsResponseData(), cancellation: ct);
    }
}
