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

/// <summary>
/// 用户字段级修改历史（query 分页），与 <see cref="GetUserChangeHistoryEndpoint"/> 数据一致。
/// 兼容旧前端或网关使用 <c>GET /api/admin/user/log?userId=&amp;page=&amp;pageSize=</c> 的调用方式。
/// </summary>
public record GetUserFieldChangeLogRequest(
    UserId UserId,
    int Page = 1,
    int PageSize = 20,
    string? Keyword = null);

/// <summary>
/// 用户字段修改记录（query: userId, page, pageSize）
/// </summary>
/// <param name="dbContext">应用数据库上下文。</param>
/// <param name="historyQuery">变更历史查询服务。</param>
public class GetUserFieldChangeLogEndpoint(ApplicationDbContext dbContext, UserChangeHistoryQuery historyQuery)
    : Endpoint<GetUserFieldChangeLogRequest, ResponseData<PagedData<UserFieldChangeRowDto>>>
{
    public override void Configure()
    {
        Tags("Users");
        Description(b => b.AutoTagOverride("Users").WithSummary("用户字段修改记录（query: userId, page, pageSize）"));
        Get("/api/admin/user/log");
        AuthSchemes(JwtBearerDefaults.AuthenticationScheme);
        Permissions(PermissionCodes.UserChangeHistoryView);
    }

    public override async Task HandleAsync(GetUserFieldChangeLogRequest req, CancellationToken ct)
    {
        var exists = await dbContext.Users.AsNoTracking().AnyAsync(u => u.Id == req.UserId, ct);
        if (!exists)
        {
            await Send.NotFoundAsync(ct);
            return;
        }

        var pageIndex = req.Page < 1 ? 1 : req.Page;
        var pageSize = req.PageSize < 1 ? 20 : Math.Min(req.PageSize, 500);
        var input = new PageRequest
        {
            PageIndex = pageIndex,
            PageSize = pageSize,
            CountTotal = true,
        };
        var data = await historyQuery.GetUserFieldChangeHistoryPagedAsync(req.UserId, input, req.Keyword, ct);
        await Send.OkAsync(data.AsResponseData(), cancellation: ct);
    }
}
