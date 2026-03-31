using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.ProductCategoryAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProductTypeAggregate;
using Ncp.Admin.Domain.AggregatesModel.SupplierAggregate;

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

    /// <summary>产品类型 ID（关联产品类型聚合）</summary>
    public ProductTypeId ProductTypeId { get; private set; } = default!;

    /// <summary>状态（是否有效）</summary>
    public bool Status { get; private set; }

    /// <summary>产品名称</summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>产品编号/货号</summary>
    public string Code { get; private set; } = string.Empty;

    /// <summary>型号</summary>
    public string Model { get; private set; } = string.Empty;

    /// <summary>单位</summary>
    public string Unit { get; private set; } = string.Empty;

    /// <summary>条码</summary>
    public string Barcode { get; private set; } = string.Empty;

    /// <summary>激活码</summary>
    public string ActivationCode { get; private set; } = string.Empty;

    /// <summary>价格标准（文本）</summary>
    public string PriceStandard { get; private set; } = string.Empty;

    /// <summary>市场销售</summary>
    public string MarketSales { get; private set; } = string.Empty;

    /// <summary>描述</summary>
    public string Description { get; private set; } = string.Empty;

    /// <summary>成本价</summary>
    public decimal CostPrice { get; private set; }

    /// <summary>客户价</summary>
    public decimal CustomerPrice { get; private set; }

    /// <summary>库存数量</summary>
    public int Qty { get; private set; }

    /// <summary>标签</summary>
    public string Tags { get; private set; } = string.Empty;

    /// <summary>功能特点</summary>
    public string Feature { get; private set; } = string.Empty;

    /// <summary>硬件配置</summary>
    public string Configuration { get; private set; } = string.Empty;

    /// <summary>使用说明</summary>
    public string Instructions { get; private set; } = string.Empty;

    /// <summary>操作流程（原安装流程）</summary>
    public string InstallProcess { get; private set; } = string.Empty;

    /// <summary>操作流程资源（上传的资源存储 Key，JSON 数组字符串）</summary>
    public string OperationProcessResources { get; private set; } = string.Empty;

    /// <summary>产品介绍</summary>
    public string Introduction { get; private set; } = string.Empty;

    /// <summary>产品介绍资源（上传的资源存储 Key，JSON 数组字符串）</summary>
    public string IntroductionResources { get; private set; } = string.Empty;

    /// <summary>图片路径/照片存储 Key</summary>
    public string ImagePath { get; private set; } = string.Empty;

    /// <summary>产品分类 ID（无分类时由端点传 new ProductCategoryId(Guid.Empty)）</summary>
    public ProductCategoryId CategoryId { get; private set; } = default!;

    /// <summary>供应商 ID（无供应商时由端点传 new SupplierId(Guid.Empty)）</summary>
    public SupplierId SupplierId { get; private set; } = default!;

    /// <summary>
    /// 创建产品（必填项由端点赋默认值，聚合内不判空）
    /// </summary>
    public Product(
        ProductTypeId productTypeId,
        bool status,
        string name,
        string code,
        string model,
        string unit,
        string barcode,
        string activationCode,
        string priceStandard,
        string marketSales,
        string description,
        decimal costPrice,
        decimal customerPrice,
        int qty,
        string tags,
        string feature,
        string configuration,
        string instructions,
        string installProcess,
        string operationProcessResources,
        string introduction,
        string introductionResources,
        string imagePath,
        ProductCategoryId categoryId,
        SupplierId supplierId)
    {
        ProductTypeId = productTypeId;
        Status = status;
        Name = name;
        Code = code;
        Model = model;
        Unit = unit;
        Barcode = barcode;
        ActivationCode = activationCode ?? string.Empty;
        PriceStandard = priceStandard ?? string.Empty;
        MarketSales = marketSales ?? string.Empty;
        Description = description;
        CostPrice = costPrice;
        CustomerPrice = customerPrice;
        Qty = qty;
        Tags = tags;
        Feature = feature;
        Configuration = configuration;
        Instructions = instructions;
        InstallProcess = installProcess;
        OperationProcessResources = operationProcessResources ?? string.Empty;
        Introduction = introduction;
        IntroductionResources = introductionResources ?? string.Empty;
        ImagePath = imagePath;
        CategoryId = categoryId;
        SupplierId = supplierId;
    }

    /// <summary>
    /// 更新产品信息
    /// </summary>
    public void Update(
        ProductTypeId productTypeId,
        bool status,
        string name,
        string code,
        string model,
        string unit,
        string barcode,
        string activationCode,
        string priceStandard,
        string marketSales,
        string description,
        decimal costPrice,
        decimal customerPrice,
        int qty,
        string tags,
        string feature,
        string configuration,
        string instructions,
        string installProcess,
        string operationProcessResources,
        string introduction,
        string introductionResources,
        string imagePath,
        ProductCategoryId categoryId,
        SupplierId supplierId)
    {
        ProductTypeId = productTypeId;
        Status = status;
        Name = name;
        Code = code;
        Model = model;
        Unit = unit;
        Barcode = barcode;
        ActivationCode = activationCode ?? string.Empty;
        PriceStandard = priceStandard ?? string.Empty;
        MarketSales = marketSales ?? string.Empty;
        Description = description;
        CostPrice = costPrice;
        CustomerPrice = customerPrice;
        Qty = qty;
        Tags = tags;
        Feature = feature;
        Configuration = configuration;
        Instructions = instructions;
        InstallProcess = installProcess;
        OperationProcessResources = operationProcessResources ?? string.Empty;
        Introduction = introduction;
        IntroductionResources = introductionResources ?? string.Empty;
        ImagePath = imagePath;
        CategoryId = categoryId;
        SupplierId = supplierId;
    }
}
