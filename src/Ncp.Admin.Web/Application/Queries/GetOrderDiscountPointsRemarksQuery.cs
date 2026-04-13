using MediatR;
using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 获取订单“优惠点数说明”备注列表（仅 TypeId=1）
/// </summary>
/// <param name="OrderId">订单 ID</param>
/// <param name="RequestUserId">当前请求用户 ID（非全部可见时按此过滤）</param>
/// <param name="CanViewAll">是否可查看全部备注</param>
public record GetOrderDiscountPointsRemarksQuery(OrderId OrderId, UserId RequestUserId, bool CanViewAll)
    : IRequest<List<OrderRemarkDto>>;

/// <inheritdoc />
public class GetOrderDiscountPointsRemarksQueryHandler(ApplicationDbContext dbContext)
    : IRequestHandler<GetOrderDiscountPointsRemarksQuery, List<OrderRemarkDto>>
{
    public async Task<List<OrderRemarkDto>> Handle(GetOrderDiscountPointsRemarksQuery request, CancellationToken cancellationToken)
    {
        var query = dbContext.OrderRemarks
            .AsNoTracking()
            .Where(r => r.OrderId == request.OrderId && r.TypeId == 1);

        if (!request.CanViewAll)
        {
            query = query.Where(r => r.UserId == request.RequestUserId);
        }

        var rawRemarks = await query
            .OrderByDescending(r => r.AddedAt)
            .Select(r => new
            {
                Id = r.Id.ToString(),
                TypeId = r.TypeId,
                Content = r.Content,
                UserId = r.UserId,
                AddedAt = r.AddedAt
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

