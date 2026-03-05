using Ncp.Admin.Domain;

namespace Ncp.Admin.Domain.AggregatesModel.ContractTypeOptionAggregate;

/// <summary>
/// 合同类型选项 ID（强类型）
/// </summary>
public partial record ContractTypeOptionId : IGuidStronglyTypedId;

/// <summary>
/// 合同类型选项聚合根：主数据，供合同与订单引用（名称、类型值、是否在订单签订公司下拉展示、排序）
/// </summary>
public class ContractTypeOption : Entity<ContractTypeOptionId>, IAggregateRoot
{
    protected ContractTypeOption() { }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 类型值（int，与合同 ContractType 等字段对应）
    /// </summary>
    public int TypeValue { get; private set; }

    /// <summary>
    /// 是否在订单签订公司选项中展示
    /// </summary>
    public bool OrderSigningCompanyOptionDisplay { get; private set; }

    /// <summary>
    /// 排序（数字越小越靠前）
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// 创建合同类型选项
    /// </summary>
    public ContractTypeOption(string name, int typeValue, bool orderSigningCompanyOptionDisplay = false, int sortOrder = 0)
    {
        Name = name;
        TypeValue = typeValue;
        OrderSigningCompanyOptionDisplay = orderSigningCompanyOptionDisplay;
        SortOrder = sortOrder;
    }

    /// <summary>
    /// 更新合同类型选项
    /// </summary>
    public void Update(string name, int typeValue, bool orderSigningCompanyOptionDisplay, int sortOrder)
    {
        Name = name;
        TypeValue = typeValue;
        OrderSigningCompanyOptionDisplay = orderSigningCompanyOptionDisplay;
        SortOrder = sortOrder;
    }
}
