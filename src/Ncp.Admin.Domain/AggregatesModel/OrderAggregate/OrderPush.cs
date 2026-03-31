using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.OrderAggregate;

/// <summary>
/// 订单推送 ID（强类型）
/// </summary>
public partial record OrderPushId : IGuidStronglyTypedId;

/// <summary>
/// 订单推送聚合根：记录订单推送信息
/// </summary>
/// <remarks>
/// 用于跟踪订单在不同系统间的推送状态和历史记录
/// </remarks>
public class OrderPush : Entity<OrderPushId>
{
    /// <summary>EF/序列化用</summary>
    protected OrderPush() { }

    /// <summary>订单 ID</summary>
    public OrderId OrderId { get; private set; } = default!;

    /// <summary>类别</summary>
    /// <remarks>推送的类型，例如：1-系统推送，2-手动推送，3-自动推送等</remarks>
    public int Type { get; private set; }

    /// <summary>推送人用户 ID</summary>
    public UserId PusherId { get; private set; } = default!;

    /// <summary>推送人姓名</summary>
    /// <remarks>冗余存储，便于直接查询和展示</remarks>
    public string PusherName { get; private set; } = string.Empty;

    /// <summary>推送时间</summary>
    public DateTimeOffset PushTime { get; private set; }

    /// <summary>进程</summary>
    /// <remarks>推送的进程状态码，例如：0-待推送，1-推送中，2-推送成功，3-推送失败等</remarks>
    public int Process { get; private set; }

    /// <summary>进程名称</summary>
    /// <remarks>进程状态的文字描述，例如："待推送"、"推送中"、"推送成功"、"推送失败"等</remarks>
    public string ProcessName { get; private set; } = string.Empty;

    /// <summary>原因</summary>
    /// <remarks>推送的原因或失败的详细信息</remarks>
    public string Reason { get; private set; } = string.Empty;

    /// <summary>
    /// 创建订单推送
    /// </summary>
    /// <param name="orderId">订单 ID</param>
    /// <param name="type">类别</param>
    /// <param name="pusherId">推送人用户 ID</param>
    /// <param name="pusherName">推送人姓名</param>
    /// <param name="process">进程</param>
    /// <param name="processName">进程名称</param>
    /// <param name="reason">原因</param>
    public OrderPush(
        OrderId orderId,
        int type,
        UserId pusherId,
        string pusherName,
        int process,
        string processName,
        string reason)
    {
        OrderId = orderId;
        Type = type;
        PusherId = pusherId;
        PusherName = pusherName;
        PushTime = DateTimeOffset.UtcNow;
        Process = process;
        ProcessName = processName;
        Reason = reason;
    }

    /// <summary>
    /// 更新订单推送
    /// </summary>
    /// <param name="type">类别</param>
    /// <param name="process">进程</param>
    /// <param name="processName">进程名称</param>
    /// <param name="reason">原因</param>
    public void Update(
        int type,
        int process,
        string processName,
        string reason)
    {
        Type = type;
        Process = process;
        ProcessName = processName;
        Reason = reason;
    }
}