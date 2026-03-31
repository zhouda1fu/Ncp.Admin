using MediatR;
using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 获取订单备注列表（财务使用：TypeId=0）
/// </summary>
public class GetOrderRemarksQuery(OrderId orderId) : IRequest<List<OrderRemarkDto>>
{
    public OrderId OrderId { get; } = orderId;
}

/// <summary>
/// 订单备注响应
/// </summary>
public class OrderRemarkDto
{
    /// <summary>备注ID</summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>备注类型ID（当前固定为 0）</summary>
    public int TypeId { get; set; }

    /// <summary>备注内容</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>创建备注人用户ID</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>添加人名称</summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>添加时间</summary>
    public DateTimeOffset AddedAt { get; set; }
}

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
            .Select(r => new OrderRemarkDto
            {
                Id = r.Id,
                TypeId = r.TypeId,
                Content = r.Content,
                UserId = r.UserId.ToString(),
                UserName = userNameMap.TryGetValue(r.UserId, out var name) ? name : string.Empty,
                AddedAt = r.AddedAt
            })
            .ToList();
    }
}

