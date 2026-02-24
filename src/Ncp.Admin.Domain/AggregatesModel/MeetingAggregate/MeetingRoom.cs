namespace Ncp.Admin.Domain.AggregatesModel.MeetingAggregate;

/// <summary>
/// 会议室ID（强类型ID）
/// </summary>
public partial record MeetingRoomId : IGuidStronglyTypedId;

/// <summary>
/// 会议室聚合根，表示可预订的会议室资源
/// </summary>
public class MeetingRoom : Entity<MeetingRoomId>, IAggregateRoot
{
    protected MeetingRoom() { }

    /// <summary>
    /// 会议室名称
    /// </summary>
    public string Name { get; private set; } = string.Empty;
    /// <summary>
    /// 位置/楼层（可选）
    /// </summary>
    public string? Location { get; private set; }
    /// <summary>
    /// 容纳人数
    /// </summary>
    public int Capacity { get; private set; }
    /// <summary>
    /// 设备说明（如投影、白板，可选）
    /// </summary>
    public string? Equipment { get; private set; }
    /// <summary>
    /// 状态：0 禁用，1 可用
    /// </summary>
    public int Status { get; private set; } = 1;
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }
    /// <summary>
    /// 最后更新时间
    /// </summary>
    public UpdateTime UpdateTime { get; private set; } = new UpdateTime(DateTimeOffset.UtcNow);

    /// <summary>
    /// 创建会议室
    /// </summary>
    /// <param name="name">名称</param>
    /// <param name="location">位置（可选）</param>
    /// <param name="capacity">容纳人数</param>
    /// <param name="equipment">设备说明（可选）</param>
    public MeetingRoom(string name, string? location, int capacity, string? equipment = null)
    {
        Name = name ?? string.Empty;
        Location = location;
        Capacity = capacity;
        Equipment = equipment;
        Status = 1;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 更新会议室信息
    /// </summary>
    /// <param name="name">名称</param>
    /// <param name="location">位置（可选）</param>
    /// <param name="capacity">容纳人数</param>
    /// <param name="equipment">设备说明（可选）</param>
    public void Update(string name, string? location, int capacity, string? equipment)
    {
        Name = name ?? string.Empty;
        Location = location;
        Capacity = capacity;
        Equipment = equipment;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 设置会议室状态（0 禁用，1 可用）
    /// </summary>
    /// <param name="status">状态值</param>
    public void SetStatus(int status)
    {
        Status = status;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }
}
