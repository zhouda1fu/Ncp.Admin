using FastEndpoints;
using FastEndpoints.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;
using Ncp.Admin.Web.Application.Queries;
using Ncp.Admin.Web.AppPermissions;
using NetCorePal.Extensions.Dto;

namespace Ncp.Admin.Web.Endpoints.Identity.Admin.UserEndpoints;

/// <summary>用户字段级修改历史分页（由操作日志请求体对比生成）。</summary>

public record GetUserChangeHistoryRequest(
    UserId UserId,
    int PageIndex = 1,
    int PageSize = 20,
    string? Keyword = null);

/// <summary>
/// 用户字段级修改历史。
/// </summary>
/// <param name="dbContext">应用数据库上下文。</param>
/// <param name="historyQuery">变更历史查询服务。</param>
public class GetUserChangeHistoryEndpoint(ApplicationDbContext dbContext, UserChangeHistoryQuery historyQuery)
    : Endpoint<GetUserChangeHistoryRequest, ResponseData<PagedData<UserFieldChangeRowDto>>>
{
    public override void Configure()
    {
        Tags("Users");
        Description(b => b.AutoTagOverride("Users").WithSummary("用户字段级修改历史"));
        Get("/api/admin/users/{userId}/change-history");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.AllApiAccess, PermissionCodes.UserChangeHistoryView);
    }

    public override async Task HandleAsync(GetUserChangeHistoryRequest req, CancellationToken ct)
    {
        var exists = await dbContext.Users.AsNoTracking().AnyAsync(u => u.Id == req.UserId, ct);
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
        var data = await historyQuery.GetUserFieldChangeHistoryPagedAsync(req.UserId, input, req.Keyword, ct);
        await Send.OkAsync(data.AsResponseData(), cancellation: ct);
    }
}
