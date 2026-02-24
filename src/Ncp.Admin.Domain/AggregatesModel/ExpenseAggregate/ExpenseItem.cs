namespace Ncp.Admin.Domain.AggregatesModel.ExpenseAggregate;

/// <summary>
/// 报销明细ID（强类型ID）
/// </summary>
public partial record ExpenseItemId : IGuidStronglyTypedId;

/// <summary>
/// 费用类型
/// </summary>
public enum ExpenseType
{
    /// <summary>
    /// 差旅
    /// </summary>
    Travel = 0,
    /// <summary>
    /// 餐饮
    /// </summary>
    Meals = 1,
    /// <summary>
    /// 住宿
    /// </summary>
    Accommodation = 2,
    /// <summary>
    /// 办公
    /// </summary>
    Office = 3,
    /// <summary>
    /// 其他
    /// </summary>
    Other = 4,
}

/// <summary>
/// 报销明细（聚合内实体，外键由 EF 影子属性映射）
/// </summary>
public class ExpenseItem : Entity<ExpenseItemId>
{
    protected ExpenseItem() { }

    /// <summary>
    /// 费用类型
    /// </summary>
    public ExpenseType Type { get; private set; }
    /// <summary>
    /// 金额
    /// </summary>
    public decimal Amount { get; private set; }
    /// <summary>
    /// 说明
    /// </summary>
    public string Description { get; private set; } = string.Empty;
    /// <summary>
    /// 发票链接（可选）
    /// </summary>
    public string? InvoiceUrl { get; private set; }

    /// <summary>
    /// 创建报销明细（仅供聚合内部使用）
    /// </summary>
    internal ExpenseItem(ExpenseType type, decimal amount, string description, string? invoiceUrl = null)
    {
        Type = type;
        Amount = amount;
        Description = description ?? string.Empty;
        InvoiceUrl = invoiceUrl;
    }
}
