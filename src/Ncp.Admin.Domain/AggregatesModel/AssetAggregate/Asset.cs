using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain;

namespace Ncp.Admin.Domain.AggregatesModel.AssetAggregate;

/// <summary>
/// 资产ID（强类型ID）
/// </summary>
public partial record AssetId : IGuidStronglyTypedId;

/// <summary>
/// 资产状态
/// </summary>
public enum AssetStatus
{
    /// <summary>
    /// 可用
    /// </summary>
    Available = 0,
    /// <summary>
    /// 已领用
    /// </summary>
    Allocated = 1,
    /// <summary>
    /// 已报废
    /// </summary>
    Scrapped = 2,
}

/// <summary>
/// 固定资产聚合根：登记、领用、归还、盘点
/// </summary>
public class Asset : Entity<AssetId>, IAggregateRoot
{
    protected Asset() { }

    /// <summary>
    /// 资产名称
    /// </summary>
    public string Name { get; private set; } = string.Empty;
    /// <summary>
    /// 资产分类/类别
    /// </summary>
    public string Category { get; private set; } = string.Empty;
    /// <summary>
    /// 资产编号
    /// </summary>
    public string Code { get; private set; } = string.Empty;
    /// <summary>
    /// 状态
    /// </summary>
    public AssetStatus Status { get; private set; }
    /// <summary>
    /// 购置日期
    /// </summary>
    public DateTimeOffset PurchaseDate { get; private set; }
    /// <summary>
    /// 资产价值（元）
    /// </summary>
    public decimal Value { get; private set; }
    /// <summary>
    /// 备注
    /// </summary>
    public string? Remark { get; private set; }
    /// <summary>
    /// 登记人用户ID
    /// </summary>
    public UserId CreatorId { get; private set; } = default!;
    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; init; }
    /// <summary>
    /// 最后更新时间
    /// </summary>
    public UpdateTime UpdateTime { get; private set; } = new UpdateTime(DateTimeOffset.UtcNow);

    /// <summary>
    /// 创建资产（可用状态）
    /// </summary>
    public Asset(string name, string category, string code, DateTimeOffset purchaseDate, decimal value, UserId creatorId, string? remark = null)
    {
        Name = name ?? string.Empty;
        Category = category ?? string.Empty;
        Code = code ?? string.Empty;
        PurchaseDate = purchaseDate;
        Value = value;
        CreatorId = creatorId;
        Remark = remark;
        Status = AssetStatus.Available;
        CreatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 更新资产信息（仅可用可改）
    /// </summary>
    public void Update(string name, string category, string code, DateTimeOffset purchaseDate, decimal value, string? remark = null)
    {
        if (Status != AssetStatus.Available)
            throw new KnownException("仅可用状态可修改", ErrorCodes.AssetInvalidStatus);
        Name = name ?? string.Empty;
        Category = category ?? string.Empty;
        Code = code ?? string.Empty;
        PurchaseDate = purchaseDate;
        Value = value;
        Remark = remark;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 标记为已领用（由 AssetAllocation 创建时调用，此处仅改状态）
    /// </summary>
    public void MarkAllocated()
    {
        if (Status != AssetStatus.Available)
            throw new KnownException("仅可用可领用", ErrorCodes.AssetInvalidStatus);
        Status = AssetStatus.Allocated;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 标记为可用（归还时调用）
    /// </summary>
    public void MarkAvailable()
    {
        if (Status != AssetStatus.Allocated)
            throw new KnownException("仅已领用可归还", ErrorCodes.AssetInvalidStatus);
        Status = AssetStatus.Available;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }

    /// <summary>
    /// 报废
    /// </summary>
    public void Scrap()
    {
        if (Status == AssetStatus.Scrapped)
            throw new KnownException("已报废", ErrorCodes.AssetInvalidStatus);
        Status = AssetStatus.Scrapped;
        UpdateTime = new UpdateTime(DateTimeOffset.UtcNow);
    }
}
