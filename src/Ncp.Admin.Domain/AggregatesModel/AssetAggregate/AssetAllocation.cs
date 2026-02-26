using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain;

namespace Ncp.Admin.Domain.AggregatesModel.AssetAggregate;

/// <summary>
/// 资产领用记录ID（强类型ID）
/// </summary>
public partial record AssetAllocationId : IGuidStronglyTypedId;

/// <summary>
/// 资产领用记录聚合根：领用、归还
/// </summary>
public class AssetAllocation : Entity<AssetAllocationId>, IAggregateRoot
{
    protected AssetAllocation() { }

    /// <summary>
    /// 资产ID
    /// </summary>
    public AssetId AssetId { get; private set; } = default!;
    /// <summary>
    /// 领用人用户ID
    /// </summary>
    public UserId UserId { get; private set; } = default!;
    /// <summary>
    /// 领用时间
    /// </summary>
    public DateTimeOffset AllocatedAt { get; private set; }
    /// <summary>
    /// 归还时间（未归还可空）
    /// </summary>
    public DateTimeOffset? ReturnedAt { get; private set; }
    /// <summary>
    /// 备注
    /// </summary>
    public string? Note { get; private set; }

    /// <summary>
    /// 创建领用记录（领用）
    /// </summary>
    public AssetAllocation(AssetId assetId, UserId userId, string? note = null)
    {
        AssetId = assetId;
        UserId = userId;
        AllocatedAt = DateTimeOffset.UtcNow;
        Note = note;
    }

    /// <summary>
    /// 归还
    /// </summary>
    public void Return()
    {
        if (ReturnedAt.HasValue)
            throw new KnownException("已归还", ErrorCodes.AssetInvalidStatus);
        ReturnedAt = DateTimeOffset.UtcNow;
    }
}
