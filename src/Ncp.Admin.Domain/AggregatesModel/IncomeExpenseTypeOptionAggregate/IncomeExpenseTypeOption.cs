using Ncp.Admin.Domain;

namespace Ncp.Admin.Domain.AggregatesModel.IncomeExpenseTypeOptionAggregate;

/// <summary>
/// 收支类型选项 ID（强类型）
/// </summary>
public partial record IncomeExpenseTypeOptionId : IGuidStronglyTypedId;

/// <summary>
/// 收支类型选项聚合根：主数据，供合同引用（名称、类型值、排序）
/// </summary>
public class IncomeExpenseTypeOption : Entity<IncomeExpenseTypeOptionId>, IAggregateRoot
{
    protected IncomeExpenseTypeOption() { }

    /// <summary>
    /// 名称
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// 类型值（int，与合同 IncomeExpenseType 等字段对应）
    /// </summary>
    public int TypeValue { get; private set; }

    /// <summary>
    /// 排序（数字越小越靠前）
    /// </summary>
    public int SortOrder { get; private set; }

    /// <summary>
    /// 创建收支类型选项
    /// </summary>
    public IncomeExpenseTypeOption(string name, int typeValue, int sortOrder = 0)
    {
        Name = name;
        TypeValue = typeValue;
        SortOrder = sortOrder;
    }

    /// <summary>
    /// 更新收支类型选项
    /// </summary>
    public void Update(string name, int typeValue, int sortOrder)
    {
        Name = name;
        TypeValue = typeValue;
        SortOrder = sortOrder;
    }
}
