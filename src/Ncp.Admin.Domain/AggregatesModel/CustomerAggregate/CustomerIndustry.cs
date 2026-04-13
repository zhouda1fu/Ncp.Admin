using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.IndustryAggregate;

namespace Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;

/// <summary>
/// 客户-行业关联 ID（强类型）
/// </summary>
public partial record CustomerIndustryId : IGuidStronglyTypedId;

/// <summary>
/// 客户-行业关联子实体，仅通过 <see cref="CustomerIndustry.CreatePending"/> 挂入 <see cref="Customer.Industries"/>，由 EF 修复 <see cref="CustomerId"/>。
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
    /// 挂入 <see cref="Customer.Industries"/>：仅填 <see cref="IndustryId"/>，<see cref="CustomerId"/> 由 EF 在持久化前根据父子关系修复（新建或更新时均适用）。
    /// </summary>
    internal static CustomerIndustry CreatePending(IndustryId industryId)
    {
        return new CustomerIndustry
        {
            IndustryId = industryId,
        };
    }
}
