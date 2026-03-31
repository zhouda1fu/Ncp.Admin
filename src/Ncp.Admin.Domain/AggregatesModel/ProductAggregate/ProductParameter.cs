using Ncp.Admin.Domain;

namespace Ncp.Admin.Domain.AggregatesModel.ProductAggregate;

/// <summary>
/// 产品参数 ID（强类型）
/// </summary>
public partial record ProductParameterId : IGuidStronglyTypedId;

/// <summary>
/// 产品参数聚合根：按产品+年份存储参数说明（如年度参数文档）
/// </summary>
public class ProductParameter : Entity<ProductParameterId>, IAggregateRoot
{
    /// <summary>EF/序列化用</summary>
    protected ProductParameter() { }

    /// <summary>所属产品 ID</summary>
    public ProductId ProductId { get; private set; } = default!;

    /// <summary>参数年份（如 "2024"）</summary>
    public string Year { get; private set; } = string.Empty;

    /// <summary>参数内容/描述</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>
    /// 创建产品参数
    /// </summary>
    public ProductParameter(ProductId productId, string year, string description)
    {
        ProductId = productId;
        Year = year ?? string.Empty;
        Description = description ?? string.Empty;
    }

    /// <summary>
    /// 更新参数内容
    /// </summary>
    public void Update(string year, string description)
    {
        Year = year ?? string.Empty;
        Description = description ?? string.Empty;
    }
}
