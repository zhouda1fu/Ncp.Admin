using Ncp.Admin.Domain;

namespace Ncp.Admin.Domain.AggregatesModel.ProductAggregate;

/// <summary>
/// 产品 ID（强类型）
/// </summary>
public partial record ProductId : IGuidStronglyTypedId;

/// <summary>
/// 产品聚合根：供订单明细关联
/// </summary>
public class Product : Entity<ProductId>, IAggregateRoot
{
    /// <summary>EF/序列化用</summary>
    protected Product() { }

    /// <summary>产品名称</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>产品编号/货号</summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>型号</summary>
    public string Model { get; private set; } = string.Empty;

    /// <summary>单位</summary>
    public string Unit { get; private set; } = string.Empty;

    /// <summary>
    /// 创建产品
    /// </summary>
    public Product(string name, string code, string model, string unit)
    {
        Name = name ?? string.Empty;
        Code = code ?? string.Empty;
        Model = model ?? string.Empty;
        Unit = unit ?? string.Empty;
    }

    /// <summary>
    /// 更新产品信息
    /// </summary>
    public void Update(string name, string code, string model, string unit)
    {
        Name = name ?? string.Empty;
        Code = code ?? string.Empty;
        Model = model ?? string.Empty;
        Unit = unit ?? string.Empty;
    }
}
