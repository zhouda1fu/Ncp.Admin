using MediatR;
using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 订单备注项（财务备注 / 优惠点数说明等共用）
/// </summary>
/// <param name="Id">备注记录 ID</param>
/// <param name="TypeId">备注类型（0 表示财务备注）</param>
/// <param name="Content">备注正文</param>
/// <param name="UserId">添加人用户 ID</param>
/// <param name="UserName">添加人显示名称</param>
/// <param name="AddedAt">添加时间</param>
public record OrderRemarkDto(
    string Id,
    int TypeId,
    string Content,
    string UserId,
    string UserName,
    DateTimeOffset AddedAt);

/// <summary>
/// 获取订单备注列表（财务使用：TypeId=0）
/// </summary>
/// <param name="OrderId">订单 ID</param>
public record GetOrderRemarksQuery(OrderId OrderId) : IRequest<List<OrderRemarkDto>>;

/// <summary>
/// 获取订单备注列表查询处理器
/// </summary>
public class GetOrderRemarksQueryHandler(ApplicationDbContext dbContext)
    : IRequestHandler<GetOrderRemarksQuery, List<OrderRemarkDto>>
{
    public async Task<List<OrderRemarkDto>> Handle(GetOrderRemarksQuery request, CancellationToken cancellationToken)
    {
        var rawRemarks = await dbContext.OrderRemarks
            .AsNoTracking()
            .Where(r => r.OrderId == request.OrderId && r.TypeId == 0)
            .OrderByDescending(r => r.AddedAt)
            .Select(r => new
            {
                Id = r.Id.ToString(),
                TypeId = r.TypeId,
                Content = r.Content,
                UserId = r.UserId,
                AddedAt = r.AddedAt,
            })
            .ToListAsync(cancellationToken);

        var userIdList = rawRemarks
            .Select(x => x.UserId)
            .Distinct()
            .ToList();

        var userNameMap = await dbContext.Users
            .AsNoTracking()
            .Where(u => userIdList.Contains(u.Id))
            .Select(u => new { u.Id, u.RealName })
            .ToDictionaryAsync(k => k.Id, v => v.RealName, cancellationToken);

        return rawRemarks
            .Select(r => new OrderRemarkDto(
                r.Id,
                r.TypeId,
                r.Content,
                r.UserId.ToString(),
                userNameMap.TryGetValue(r.UserId, out var name) ? name : string.Empty,
                r.AddedAt))
            .ToList();
    }
}
