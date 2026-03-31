using MediatR;
using Microsoft.EntityFrameworkCore;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Infrastructure;

namespace Ncp.Admin.Web.Application.Queries;

/// <summary>
/// 获取订单推送记录查询
/// </summary>
public class GetOrderPushRecordsQuery : IRequest<List<OrderPushRecordResponse>>
{
    /// <summary>订单ID</summary>
    public OrderId OrderId { get; }

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="orderId">订单ID</param>
    public GetOrderPushRecordsQuery(OrderId orderId)
    {
        OrderId = orderId;
    }
}

/// <summary>
/// 订单推送记录响应
/// </summary>
public class OrderPushRecordResponse
{
    /// <summary>推送ID</summary>
    public string PushId { get; set; } = string.Empty;

    /// <summary>操作部门</summary>
    public string? DeptName { get; set; } = string.Empty;

    /// <summary>操作人</summary>
    public string PusherName { get; set; } = string.Empty;

    /// <summary>操作人用户ID</summary>
    public UserId PusherId { get; set; } = default!;

    /// <summary>操作时间</summary>
    public DateTimeOffset PushTime { get; set; }

    /// <summary>操作信息</summary>
    public string ProcessName { get; set; } = string.Empty;

    /// <summary>驳回原因</summary>
    public string Reason { get; set; } = string.Empty;
}

/// <summary>
/// 获取订单推送记录查询处理器
/// </summary>
public class GetOrderPushRecordsQueryHandler(ApplicationDbContext dbContext) : IRequestHandler<GetOrderPushRecordsQuery, List<OrderPushRecordResponse>>
{
    public async Task<List<OrderPushRecordResponse>> Handle(GetOrderPushRecordsQuery request, CancellationToken cancellationToken)
    {
        var pushes = await dbContext.OrderPushes
            .Where(x => x.OrderId == request.OrderId && x.Type == 1)
            .OrderByDescending(x => x.PushTime)
            .Select(x => new OrderPushRecordResponse
            {
                PushId = x.Id.ToString(),
                PusherId = x.PusherId,
                PusherName = x.PusherName,
                PushTime = x.PushTime,
                ProcessName = x.ProcessName,
                Reason = x.Reason
            })
            .ToListAsync(cancellationToken);

        var userIds = pushes.Select(p => p.PusherId).Distinct().ToList();

        var userDeptMap = await dbContext.UserDepts
            .Where(ud => userIds.Contains(ud.UserId))
            .Select(ud => new { UserId = ud.UserId, DeptName = ud.DeptName })
            .ToDictionaryAsync(ud => ud.UserId, ud => ud.DeptName, cancellationToken);

        foreach (var push in pushes)
        {
            if (userDeptMap.TryGetValue(push.PusherId, out var deptName))
            {
                push.DeptName = deptName;
            }
        }

        return pushes;
    }
}
