using Ncp.Admin.Domain.DomainEvents;

namespace Ncp.Admin.Domain.AggregatesModel.VehicleAggregate;

/// <summary>
/// 车辆ID（强类型ID）
/// </summary>
public partial record VehicleId : IGuidStronglyTypedId;

/// <summary>
/// 车辆状态
/// </summary>
public enum VehicleStatus
{
    /// <summary>
    /// 可用
    /// </summary>
    Available = 0,
    /// <summary>
    /// 已禁用
    /// </summary>
    Disabled = 1,
}

/// <summary>
/// 车辆聚合根：公司用车资源
/// </summary>
public class Vehicle : Entity<VehicleId>, IAggregateRoot
{
    protected Vehicle() { }

    /// <summary>
    /// 车牌号
    /// </summary>
    public string PlateNumber { get; private set; } = string.Empty;
    /// <summary>
    /// 车型/品牌型号
    /// </summary>
    public string Model { get; private set; } = string.Empty;
    /// <summary>
    /// 状态
    /// </summary>
    public VehicleStatus Status { get; private set; } = VehicleStatus.Available;
    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; private set; }
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }
    /// <summary>
    /// 最后更新时间
    /// </summary>
    public UpdateTime UpdateTime { get; private set; } = new UpdateTime(DateTimeOffset.UtcNow);

    /// <summary>
    /// 是否软删
    /// </summary>
    public Deleted IsDeleted { get; private set; } = new Deleted(false);

    /// <summary>
    /// 并发版本
    /// </summary>
    public RowVersion RowVersion { get; private set; } = new RowVersion(0);

    /// <summary>
    /// 创建车辆
    /// </summary>
    public Vehicle(string plateNumber, string model, string? remark = null)
    {
        PlateNumber = plateNumber ;
        Model = model ;
        Remark = remark;
        Status = VehicleStatus.Available;
        CreatedAt = DateTimeOffset.UtcNow;
        AddDomainEvent(new VehicleCreatedDomainEvent(this));
    }

    /// <summary>
    /// 更新车辆信息
    /// </summary>
    public void Update(string plateNumber, string model, string? remark = null)
    {
        PlateNumber = plateNumber ;
        Model = model ;
        Remark = remark;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
        AddDomainEvent(new VehicleUpdatedDomainEvent(this));
    }

    /// <summary>
    /// 设置状态
    /// </summary>
    public void SetStatus(VehicleStatus status)
    {
        Status = status;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
        AddDomainEvent(new VehicleStatusChangedDomainEvent(this));
    }
}
