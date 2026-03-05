using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.DomainEvents.OrderEvents;

namespace Ncp.Admin.Domain.AggregatesModel.OrderAggregate;

/// <summary>
/// 订单 ID（强类型）
/// </summary>
public partial record OrderId : IGuidStronglyTypedId;

/// <summary>
/// 订单类型：0 销售 1 售后 2 样品 3 普测
/// </summary>
public enum OrderType
{
    /// <summary>销售</summary>
    Sales = 0,
    /// <summary>售后</summary>
    AfterSales = 1,
    /// <summary>样品</summary>
    Sample = 2,
    /// <summary>普测</summary>
    GeneralTest = 3,
}

/// <summary>
/// 订单状态：1 审核中 2 已下单 3 已完成 4 已驳回 5 未到款
/// </summary>
public enum OrderStatus
{
    /// <summary>审核中</summary>
    PendingAudit = 1,
    /// <summary>已下单</summary>
    Ordered = 2,
    /// <summary>已完成</summary>
    Completed = 3,
    /// <summary>已驳回</summary>
    Rejected = 4,
    /// <summary>未到款</summary>
    Unpaid = 5,
}

/// <summary>
/// 订单聚合根：主表信息与明细行，关联客户、项目、合同
/// </summary>
public class Order : Entity<OrderId>, IAggregateRoot
{
    /// <summary>EF/序列化用</summary>
    protected Order() { }

    /// <summary>订单明细列表</summary>
    public virtual ICollection<OrderItem> Items { get; } = [];

    /// <summary>客户 ID（必填）</summary>
    public CustomerId CustomerId { get; private set; } = default!;

    /// <summary>客户名称（冗余，便于列表/展示）</summary>
    public string CustomerName { get; private set; } = string.Empty;

    /// <summary>项目 ID（必填）</summary>
    public ProjectId ProjectId { get; private set; } = default!;

    /// <summary>合同 ID（必填）</summary>
    public ContractId ContractId { get; private set; } = default!;

    /// <summary>订单编号</summary>
    public string OrderNumber { get; private set; } = string.Empty;

    /// <summary>订单类型</summary>
    public OrderType Type { get; private set; }

    /// <summary>订单状态</summary>
    public OrderStatus Status { get; private set; }

    /// <summary>订单金额</summary>
    public decimal Amount { get; private set; }

    /// <summary>订单备注</summary>
    public string Remark { get; private set; } = string.Empty;

    /// <summary>负责人用户 ID（业务经理）</summary>
    public UserId OwnerId { get; private set; } = default!;

    /// <summary>负责人姓名（冗余）</summary>
    public string OwnerName { get; private set; } = string.Empty;

    /// <summary>部门 ID（必填）</summary>
    public DeptId DeptId { get; private set; } = default!;

    /// <summary>部门名称（冗余）</summary>
    public string DeptName { get; private set; } = string.Empty;

    /// <summary>项目联系人</summary>
    public string ProjectContactName { get; private set; } = string.Empty;

    /// <summary>项目联系方式</summary>
    public string ProjectContactPhone { get; private set; } = string.Empty;

    /// <summary>质保期</summary>
    public string Warranty { get; private set; } = string.Empty;

    /// <summary>合同签订公司</summary>
    public string ContractSigningCompany { get; private set; } = string.Empty;

    /// <summary>合同受托方</summary>
    public string ContractTrustee { get; private set; } = string.Empty;

    /// <summary>是否需要发票</summary>
    public bool NeedInvoice { get; private set; }

    /// <summary>安装费</summary>
    public decimal InstallationFee { get; private set; }

    /// <summary>预计运费</summary>
    public decimal EstimatedFreight { get; private set; }

    /// <summary>合同文件列表 JSON，格式：[{ path, fileName, size, format, updatedAt }]</summary>
    public string ContractFilesJson { get; private set; } = "[]";

    /// <summary>选择合同（上传的合同文件 ID，无则为空）</summary>
    public string SelectedContractFileId { get; private set; } = string.Empty;

    /// <summary>是否发货</summary>
    public bool IsShipped { get; private set; }

    /// <summary>到款情况</summary>
    public string PaymentStatus { get; private set; } = string.Empty;

    /// <summary>合同非公司模板</summary>
    public bool ContractNotCompanyTemplate { get; private set; }

    /// <summary>合同优惠</summary>
    public decimal ContractDiscount { get; private set; }

    /// <summary>合同金额</summary>
    public decimal ContractAmount { get; private set; }

    /// <summary>收货联系人</summary>
    public string ReceiverName { get; private set; } = string.Empty;

    /// <summary>收货电话</summary>
    public string ReceiverPhone { get; private set; } = string.Empty;

    /// <summary>收货地址</summary>
    public string ReceiverAddress { get; private set; } = string.Empty;

    /// <summary>付款日期</summary>
    public DateTimeOffset PayDate { get; private set; }

    /// <summary>发货/交付日期</summary>
    public DateTimeOffset DeliveryDate { get; private set; }

    /// <summary>是否软删</summary>
    public Deleted IsDeleted { get; private set; } = new Deleted(false);

    /// <summary>删除时间</summary>
    public DeletedTime DeletedAt { get; private set; } = new DeletedTime(DateTimeOffset.UtcNow);

    /// <summary>创建人用户 ID</summary>
    public UserId CreatorId { get; private set; } = default!;

    /// <summary>创建时间</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>最后更新时间</summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>
    /// 创建订单
    /// </summary>
    public static Order Create(
        CustomerId customerId,
        string customerName,
        ProjectId projectId,
        ContractId contractId,
        string orderNumber,
        OrderType type,
        OrderStatus status,
        decimal amount,
        string remark,
        UserId ownerId,
        string ownerName,
        DeptId deptId,
        string deptName,
        string projectContactName,
        string projectContactPhone,
        string warranty,
        string contractSigningCompany,
        string contractTrustee,
        bool needInvoice,
        decimal installationFee,
        decimal estimatedFreight,
        string contractFilesJson,
        string selectedContractFileId,
        bool isShipped,
        string paymentStatus,
        bool contractNotCompanyTemplate,
        decimal contractDiscount,
        decimal contractAmount,
        string receiverName,
        string receiverPhone,
        string receiverAddress,
        DateTimeOffset payDate,
        DateTimeOffset deliveryDate,
        UserId creatorId,
        IEnumerable<OrderItemData> items)
    {
        var order = new Order
        {
            CustomerId = customerId,
            CustomerName = customerName ?? string.Empty,
            ProjectId = projectId,
            ContractId = contractId,
            OrderNumber = orderNumber ?? string.Empty,
            Type = type,
            Status = status,
            Amount = amount,
            Remark = remark ?? string.Empty,
            OwnerId = ownerId,
            OwnerName = ownerName ?? string.Empty,
            DeptId = deptId,
            DeptName = deptName ?? string.Empty,
            ProjectContactName = projectContactName ?? string.Empty,
            ProjectContactPhone = projectContactPhone ?? string.Empty,
            Warranty = warranty ?? string.Empty,
            ContractSigningCompany = contractSigningCompany ?? string.Empty,
            ContractTrustee = contractTrustee ?? string.Empty,
            NeedInvoice = needInvoice,
            InstallationFee = installationFee,
            EstimatedFreight = estimatedFreight,
            ContractFilesJson = contractFilesJson ?? "[]",
            SelectedContractFileId = selectedContractFileId ?? string.Empty,
            IsShipped = isShipped,
            PaymentStatus = paymentStatus ?? string.Empty,
            ContractNotCompanyTemplate = contractNotCompanyTemplate,
            ContractDiscount = contractDiscount,
            ContractAmount = contractAmount,
            ReceiverName = receiverName ?? string.Empty,
            ReceiverPhone = receiverPhone ?? string.Empty,
            ReceiverAddress = receiverAddress ?? string.Empty,
            PayDate = payDate,
            DeliveryDate = deliveryDate,
            IsDeleted = new Deleted(false),
            DeletedAt = new DeletedTime(DateTimeOffset.UtcNow),
            CreatorId = creatorId,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = default,
        };
        order.AddDomainEvent(new OrderCreatedDomainEvent(order));
        foreach (var item in items)
            order.Items.Add(OrderItem.Create(item));
        return order;
    }

    /// <summary>
    /// 更新订单主表信息与明细（替换全部明细）
    /// </summary>
    public void Update(
        string customerName,
        ProjectId projectId,
        ContractId contractId,
        string orderNumber,
        OrderType type,
        OrderStatus status,
        decimal amount,
        string remark,
        UserId ownerId,
        string ownerName,
        DeptId deptId,
        string deptName,
        string projectContactName,
        string projectContactPhone,
        string warranty,
        string contractSigningCompany,
        string contractTrustee,
        bool needInvoice,
        decimal installationFee,
        decimal estimatedFreight,
        string contractFilesJson,
        string selectedContractFileId,
        bool isShipped,
        string paymentStatus,
        bool contractNotCompanyTemplate,
        decimal contractDiscount,
        decimal contractAmount,
        string receiverName,
        string receiverPhone,
        string receiverAddress,
        DateTimeOffset payDate,
        DateTimeOffset deliveryDate,
        IEnumerable<OrderItemData> items)
    {
        CustomerName = customerName ?? string.Empty;
        ProjectId = projectId;
        ContractId = contractId;
        OrderNumber = orderNumber ?? string.Empty;
        Type = type;
        Status = status;
        Amount = amount;
        Remark = remark ?? string.Empty;
        OwnerId = ownerId;
        OwnerName = ownerName ?? string.Empty;
        DeptId = deptId;
        DeptName = deptName ?? string.Empty;
        ProjectContactName = projectContactName ?? string.Empty;
        ProjectContactPhone = projectContactPhone ?? string.Empty;
        Warranty = warranty ?? string.Empty;
        ContractSigningCompany = contractSigningCompany ?? string.Empty;
        ContractTrustee = contractTrustee ?? string.Empty;
        NeedInvoice = needInvoice;
        InstallationFee = installationFee;
        EstimatedFreight = estimatedFreight;
        ContractFilesJson = contractFilesJson ?? "[]";
        SelectedContractFileId = selectedContractFileId ?? string.Empty;
        IsShipped = isShipped;
        PaymentStatus = paymentStatus ?? string.Empty;
        ContractNotCompanyTemplate = contractNotCompanyTemplate;
        ContractDiscount = contractDiscount;
        ContractAmount = contractAmount;
        ReceiverName = receiverName ?? string.Empty;
        ReceiverPhone = receiverPhone ?? string.Empty;
        ReceiverAddress = receiverAddress ?? string.Empty;
        PayDate = payDate;
        DeliveryDate = deliveryDate;
        UpdatedAt = DateTimeOffset.UtcNow;
        Items.Clear();
        foreach (var item in items)
            Items.Add(OrderItem.Create(item));
        AddDomainEvent(new OrderUpdatedDomainEvent(this));
    }

    /// <summary>
    /// 软删除
    /// </summary>
    public void MarkDeleted()
    {
        IsDeleted = true;
        DeletedAt = new DeletedTime(DateTimeOffset.UtcNow);
        AddDomainEvent(new OrderDeletedDomainEvent(this));
    }
}

/// <summary>
/// 订单明细行创建/更新用数据（值对象），所有字段必填
/// </summary>
public record OrderItemData(
    ProductId ProductId,
    string ProductName,
    string Model,
    string Number,
    int Qty,
    string Unit,
    decimal Price,
    decimal Amount,
    string Remark);
