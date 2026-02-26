using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.IndustryAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;

/// <summary>
/// 客户-行业关联 ID（强类型）
/// </summary>
public partial record CustomerIndustryId : IGuidStronglyTypedId;

/// <summary>
/// 客户-行业关联子实体，通过 <see cref="Customer"/> 的 SetIndustries/Update 维护
/// </summary>
public class CustomerIndustry : Entity<CustomerIndustryId>
{
    /// <summary>
    /// EF/序列化用
    /// </summary>
    protected CustomerIndustry() { }

    /// <summary>
    /// 所属客户 ID
    /// </summary>
    public CustomerId CustomerId { get; private set; } = default!;

    /// <summary>
    /// 行业 ID
    /// </summary>
    public IndustryId IndustryId { get; private set; } = default!;

    /// <summary>
    /// 创建客户-行业关联（由聚合根调用）
    /// </summary>
    internal static CustomerIndustry Create(CustomerId customerId, IndustryId industryId)
    {
        return new CustomerIndustry
        {
            CustomerId = customerId,
            IndustryId = industryId,
        };
    }
}
