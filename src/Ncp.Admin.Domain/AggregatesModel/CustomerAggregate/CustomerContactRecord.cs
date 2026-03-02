using Ncp.Admin.Domain.AggregatesModel.UserAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;

/// <summary>
/// 客户联系记录 ID（强类型）
/// </summary>
public partial record CustomerContactRecordId : IGuidStronglyTypedId;

/// <summary>
/// 客户联系记录子实体，通过 <see cref="Customer"/> 的 AddContactRecord/RemoveContactRecord 维护
/// </summary>
public class CustomerContactRecord : Entity<CustomerContactRecordId>
{
    /// <summary>
    /// EF/序列化用
    /// </summary>
    protected CustomerContactRecord() { }

    /// <summary>
    /// 所属客户 ID
    /// </summary>
    public CustomerId CustomerId { get; private set; } = default!;

    /// <summary>
    /// 记录时间
    /// </summary>
    public DateTimeOffset RecordAt { get; private set; }

    /// <summary>
    /// 联系类型（如：电话、上门拜访、微信等）
    /// </summary>
    public string RecordType { get; private set; } = string.Empty;

    /// <summary>
    /// 记录内容
    /// </summary>
    public string Content { get; private set; } = string.Empty;

    /// <summary>
    /// 记录人用户 ID
    /// </summary>
    public UserId? RecorderId { get; private set; }

    /// <summary>
    /// 记录人姓名（冗余）
    /// </summary>
    public string RecorderName { get; private set; } = string.Empty;

    /// <summary>
    /// 创建
    /// </summary>
    public static CustomerContactRecord Create(
        CustomerId customerId,
        DateTimeOffset recordAt,
        string recordType,
        string content,
        UserId? recorderId,
        string recorderName)
    {
        return new CustomerContactRecord
        {
            CustomerId = customerId,
            RecordAt = recordAt.ToUniversalTime(),
            RecordType = recordType ?? string.Empty,
            Content = content ?? string.Empty,
            RecorderId = recorderId,
            RecorderName = recorderName ?? string.Empty,
        };
    }
}
