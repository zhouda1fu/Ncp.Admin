using Ncp.Admin.Domain;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;
using Ncp.Admin.Domain.AggregatesModel.CustomerAggregate;
using Ncp.Admin.Domain.AggregatesModel.DeptAggregate;
using Ncp.Admin.Domain.AggregatesModel.OrderInvoiceTypeOptionAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProductAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProductCategoryAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProductTypeAggregate;
using Ncp.Admin.Domain.AggregatesModel.ProjectAggregate;
using Ncp.Admin.Domain.AggregatesModel.OrderLogisticsCompanyAggregate;
using Ncp.Admin.Domain.AggregatesModel.OrderLogisticsMethodAggregate;
using Ncp.Admin.Domain.AggregatesModel.UserAggregate;
using Ncp.Admin.Domain.AggregatesModel.WorkflowInstanceAggregate;
using Ncp.Admin.Domain.DomainEvents;

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
/// 订单状态：0 草稿 1 审核中 2 已下单 3 已完成 4 已驳回 5 未到款
/// </summary>
public enum OrderStatus
{
    /// <summary>草稿</summary>
    Draft = 0,
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
/// 到款状态
/// </summary>
public enum PaymentStatus
{
    /// <summary>已到全款</summary>
    FullPayment = 0,
    /// <summary>未到全款</summary>
    PartialPayment = 1,
    /// <summary>有分期未到全款加急发货</summary>
    InstallmentUrgent = 2,
    /// <summary>待确认</summary>
    PendingConfirmation = 3,
}

/// <summary>
/// 选择合同
/// </summary>
public enum SelectedContractFileId
{
    /// <summary>否</summary>
    No = 0,
    /// <summary>是</summary>
    Yes = 1,
}

/// <summary>
/// 物流支付方式
/// </summary>
public enum LogisticsPaymentMethodId
{
    /// <summary>到款未发货</summary>
    Not_Yet_Shipped = 0,
    /// <summary>到款已发货</summary>
    Shipped = 1,
}
/// <summary>
/// 仓库状态 0:未推送 1未查看 2已查看 3已分配 4已发货
/// </summary>
public enum WarehouseStatus
{
    /// <summary>未推送</summary>
    NotPushed = 0,
    /// <summary>未查看</summary>
    NotViewed = 1,
    /// <summary>已查看</summary>
    Viewed = 2,
    /// <summary>已分配</summary>
    Allocated = 3,
    /// <summary>已发货</summary>
    Shipped = 4,
}



/// <summary>
/// 订单聚合根：主表信息与明细行，关联客户、项目、合同
/// </summary>
public class Order : Entity<OrderId>, IAggregateRoot
{
    /// <summary>EF/序列化用</summary>
    protected Order() { }


    #region 订单属性

    /// <summary>
    /// 流程进度
    /// </summary>
    public int Process { get; private set; }

    /// <summary>订单编号</summary>
    public string OrderNumber { get; private set; } = string.Empty;

    /// <summary>订单类型</summary>
    public OrderType Type { get; private set; }

    /// <summary>订单状态</summary>
    public OrderStatus Status { get; private set; }

    #endregion


    #region 营销 
    /// <summary>订单明细列表（EF 用 virtual ICollection；业务上仅通过聚合方法修改）</summary>
    public virtual ICollection<OrderItem> Items { get; } = [];

    /// <summary>按产品分类维度的合同优惠（表 order_band）</summary>
    public virtual ICollection<OrderCategory> Categories { get; } = [];

    /// <summary>订单备注列表（表 order_remark）</summary>
    public virtual ICollection<OrderRemark> Remarks { get; } = [];

    /// <summary>客户 ID（必填）</summary>
    public CustomerId CustomerId { get; private set; } = default!;

    /// <summary>客户名称（冗余，便于列表/展示）</summary>
    public string CustomerName { get; private set; } = string.Empty;

    /// <summary>项目 ID（必填）</summary>
    public ProjectId ProjectId { get; private set; } = default!;

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

    /// <summary>发票类型 ID</summary>
    public OrderInvoiceTypeOptionId InvoiceTypeId { get; private set; } = default!;

    /// <summary>安装费</summary>
    public decimal InstallationFee { get; private set; }

    /// <summary>预计运费</summary>
    public decimal EstimatedFreight { get; private set; }

    /// <summary>合同文件列表 JSON，格式：[{ path, fileName, size, format, updatedAt }]</summary>
    public string ContractFilesJson { get; private set; } = "[]";

    /// <summary>备货单列表 JSON，格式：[{ path, fileName, size, format, updatedAt }]</summary>
    public string StockFilesJson { get; private set; } = "[]";

    #endregion

    #region 财务

    /// <summary>选择合同</summary>
    public SelectedContractFileId SelectedContractFileId { get; private set; }

    /// <summary>是否发货</summary>
    public bool IsShipped { get; private set; }

    /// <summary>到款情况</summary>
    public PaymentStatus PaymentStatus { get; private set; }

    /// <summary>合同非公司模板</summary>
    public bool ContractNotCompanyTemplate { get; private set; }


    /// <summary>合同金额</summary>
    public decimal ContractAmount { get; private set; }




    #endregion



    #region 物流
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

    /// <summary>物流公司ID</summary>
    public OrderLogisticsCompanyId OrderLogisticsCompanyId { get; private set; } = default!;

    /// <summary>物流方式ID</summary>
    public OrderLogisticsMethodId OrderLogisticsMethodId { get; private set; } = default!;

    /// <summary>物流费用支付方式 到款已发货，到款未发货</summary>
    public LogisticsPaymentMethodId LogisticsPaymentMethodId { get; private set; }
    /// <summary>运单编号</summary>
    public string WaybillNumber { get; private set; } = string.Empty;
    /// <summary>运费</summary>
    public decimal ShippingFee { get; private set; }

    /// <summary>是否付运费</summary>
    public bool ShippingFeeIsPay { get; private set; }
    /// <summary>附加费</summary>
    public decimal Surcharge { get; private set; }

    /// <summary>是否无logo</summary>
    public bool IsNoLogo { get; private set; }

    /// <summary>售后服务ID</summary>
    public string AfterSalesServiceId { get; private set; } = string.Empty;

    /// <summary>是否评估</summary>
    public bool IsAssess { get; private set; }

    /// <summary>评论</summary>
    public string Comments { get; private set; } = string.Empty;

    /// <summary>开始时间</summary>
    public DateTimeOffset StartDate { get; private set; }

    /// <summary>结束时间</summary>
    public DateTimeOffset EndDate { get; private set; }

    /// <summary>是否标红</summary>
    public bool IsRed { get; private set; }

    /// <summary>是否免费</summary>
    public bool IsFree { get; private set; }

    /// <summary>是否归还样品</summary>
    public bool IsRepay { get; private set; }

    /// <summary>归还日期</summary>
    public DateTimeOffset RepayDate { get; private set; }

    /// <summary>财务部-退还日期</summary>
    public DateTimeOffset FRepayDate { get; private set; }

    /// <summary>延迟时间</summary>
    public DateTimeOffset DelayDate { get; private set; }

    /// <summary>延迟原因</summary>
    public string DelayReason { get; private set; } = string.Empty;

    /// <summary>客户反馈</summary>
    public string Feedback { get; private set; } = string.Empty;

    /// <summary>服务内容</summary>
    public string Scontent { get; private set; } = string.Empty;

   
    #endregion

    #region 仓库
    /// <summary>配货人用户ID</summary>
    public UserId WarehousePickerId { get; private set; } = new UserId(0);

    /// <summary>仓库技术用户ID</summary>
    public UserId WarehouseTechId { get; private set; } = new UserId(0);

    /// <summary>复核人用户ID</summary>
    public UserId WarehouseReviewerId { get; private set; } = new UserId(0);

    /// <summary>仓库状态warehouse status</summary>
    public WarehouseStatus WarehouseStatus { get; private set; }

    #endregion



    /// <summary>是否软删</summary>
    public Deleted IsDeleted { get; private set; } = new Deleted(false);

    /// <summary>删除时间</summary>
    public DeletedTime DeletedAt { get; private set; } = new DeletedTime(DateTimeOffset.UtcNow);

    /// <summary>并发版本</summary>
    public RowVersion RowVersion { get; private set; } = new RowVersion(0);

    /// <summary>创建人用户 ID</summary>
    public UserId CreatorId { get; private set; } = default!;

    /// <summary>创建时间</summary>
    public DateTimeOffset CreatedAt { get; private set; }

    /// <summary>最后更新时间</summary>
    public DateTimeOffset UpdatedAt { get; private set; }

    /// <summary>关联的工作流实例ID（未关联工作流时为 <see cref="WorkflowInstanceId"/> 哨兵 <c>Guid.Empty</c>）</summary>
    public WorkflowInstanceId WorkflowInstanceId { get; private set; } = new WorkflowInstanceId(Guid.Empty);

    /// <summary>
    /// 新建订单（初始为草稿，提交审批后进入审核中，由工作流控制后续流转）；EF 使用无参 <see cref="Order" />。
    /// </summary>
    public Order(
        CustomerId customerId,
        string customerName,
        ProjectId projectId,
        string orderNumber,
        OrderType type,
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
        OrderInvoiceTypeOptionId invoiceTypeId,
        decimal installationFee,
        decimal estimatedFreight,
        string contractFilesJson,
        string stockFilesJson,
        SelectedContractFileId selectedContractFileId,
        bool isShipped,
        PaymentStatus paymentStatus,
        bool contractNotCompanyTemplate,
        decimal contractAmount,
        string receiverName,
        string receiverPhone,
        string receiverAddress,
        DateTimeOffset payDate,
        DateTimeOffset deliveryDate,
        OrderLogisticsCompanyId orderLogisticsCompanyId,
        OrderLogisticsMethodId orderLogisticsMethodId,
        LogisticsPaymentMethodId logisticsPaymentMethodId,
        string waybillNumber,
        decimal shippingFee,
        bool shippingFeeIsPay,
        decimal surcharge,
        bool isNoLogo,
        string afterSalesServiceId,
        bool isAssess,
        string comments,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        bool isRed,
        bool isFree,
        bool isRepay,
        DateTimeOffset repayDate,
        DateTimeOffset fRepayDate,
        DateTimeOffset delayDate,
        string delayReason,
        string feedback,
        string scontent,
        UserId warehousePickerId,
        UserId warehouseTechId,
        UserId warehouseReviewerId,
        WarehouseStatus warehouseStatus,
        UserId creatorId,
        IEnumerable<OrderItemData> items)
    {
        CustomerId = customerId;
        CustomerName = customerName ?? string.Empty;
        ProjectId = projectId;
        OrderNumber = orderNumber ?? string.Empty;
        Type = type;
        Status = OrderStatus.Draft;
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
        InvoiceTypeId = invoiceTypeId;
        InstallationFee = installationFee;
        EstimatedFreight = estimatedFreight;
        ContractFilesJson = contractFilesJson ?? "[]";
        StockFilesJson = stockFilesJson ?? "[]";
        SelectedContractFileId = selectedContractFileId;
        IsShipped = isShipped;
        PaymentStatus = paymentStatus;
        ContractNotCompanyTemplate = contractNotCompanyTemplate;
        ContractAmount = contractAmount;
        ReceiverName = receiverName ?? string.Empty;
        ReceiverPhone = receiverPhone ?? string.Empty;
        ReceiverAddress = receiverAddress ?? string.Empty;
        PayDate = payDate;
        DeliveryDate = deliveryDate;
        OrderLogisticsCompanyId = orderLogisticsCompanyId;
        OrderLogisticsMethodId = orderLogisticsMethodId;
        LogisticsPaymentMethodId = logisticsPaymentMethodId;
        WaybillNumber = waybillNumber ?? string.Empty;
        ShippingFee = shippingFee;
        ShippingFeeIsPay = shippingFeeIsPay;
        Surcharge = surcharge;
        IsNoLogo = isNoLogo;
        AfterSalesServiceId = afterSalesServiceId ?? string.Empty;
        IsAssess = isAssess;
        Comments = comments ?? string.Empty;
        StartDate = startDate;
        EndDate = endDate;
        IsRed = isRed;
        IsFree = isFree;
        IsRepay = isRepay;
        RepayDate = repayDate;
        FRepayDate = fRepayDate;
        DelayDate = delayDate;
        DelayReason = delayReason ?? string.Empty;
        Feedback = feedback ?? string.Empty;
        Scontent = scontent ?? string.Empty;
        WarehousePickerId = warehousePickerId;
        WarehouseTechId = warehouseTechId;
        WarehouseReviewerId = warehouseReviewerId;
        WarehouseStatus = warehouseStatus;
        IsDeleted = new Deleted(false);
        DeletedAt = new DeletedTime(DateTimeOffset.UtcNow);
        CreatorId = creatorId;
        CreatedAt = DateTimeOffset.UtcNow;
        UpdatedAt = default;
        WorkflowInstanceId = new WorkflowInstanceId(Guid.Empty);
        AddDomainEvent(new OrderCreatedDomainEvent(this));
        foreach (var item in items)
            Items.Add(new OrderItem(item));
    }

    /// <summary>
    /// 更新订单主表信息与明细（仅草稿或已驳回的订单可编辑）
    /// </summary>
    public void Update(
        string customerName,
        ProjectId projectId,
        string orderNumber,
        OrderType type,
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
        OrderInvoiceTypeOptionId invoiceTypeId,
        decimal installationFee,
        decimal estimatedFreight,
        string contractFilesJson,
        string stockFilesJson,
        SelectedContractFileId selectedContractFileId,
        bool isShipped,
        PaymentStatus paymentStatus,
        bool contractNotCompanyTemplate,
        decimal contractAmount,
        string receiverName,
        string receiverPhone,
        string receiverAddress,
        DateTimeOffset payDate,
        DateTimeOffset deliveryDate,
        OrderLogisticsCompanyId orderLogisticsCompanyId,
        OrderLogisticsMethodId orderLogisticsMethodId,
        LogisticsPaymentMethodId logisticsPaymentMethodId,
        string waybillNumber,
        decimal shippingFee,
        bool shippingFeeIsPay,
        decimal surcharge,
        bool isNoLogo,
        string afterSalesServiceId,
        bool isAssess,
        string comments,
        DateTimeOffset startDate,
        DateTimeOffset endDate,
        bool isRed,
        bool isFree,
        bool isRepay,
        DateTimeOffset repayDate,
        DateTimeOffset fRepayDate,
        DateTimeOffset delayDate,
        string delayReason,
        string feedback,
        string scontent,
        UserId warehousePickerId,
        UserId warehouseTechId,
        UserId warehouseReviewerId,
        WarehouseStatus warehouseStatus,
        IEnumerable<OrderItemData> items)
    {
        if (Status != OrderStatus.Draft && Status != OrderStatus.Rejected)
        {
            throw new KnownException("只有草稿或已驳回的订单可以修改", ErrorCodes.OrderNotRejected);
        }

        CustomerName = customerName ?? string.Empty;
        ProjectId = projectId;
        OrderNumber = orderNumber ?? string.Empty;
        Type = type;
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
        InvoiceTypeId = invoiceTypeId;
        InstallationFee = installationFee;
        EstimatedFreight = estimatedFreight;
        ContractFilesJson = contractFilesJson ?? "[]";
        StockFilesJson = stockFilesJson ?? "[]";
        SelectedContractFileId = selectedContractFileId;
        IsShipped = isShipped;
        PaymentStatus = paymentStatus;
        ContractNotCompanyTemplate = contractNotCompanyTemplate;
        ContractAmount = contractAmount;
        ReceiverName = receiverName ?? string.Empty;
        ReceiverPhone = receiverPhone ?? string.Empty;
        ReceiverAddress = receiverAddress ?? string.Empty;
        PayDate = payDate;
        DeliveryDate = deliveryDate;
        OrderLogisticsCompanyId = orderLogisticsCompanyId;
        OrderLogisticsMethodId = orderLogisticsMethodId;
        LogisticsPaymentMethodId = logisticsPaymentMethodId;
        WaybillNumber = waybillNumber ?? string.Empty;
        ShippingFee = shippingFee;
        ShippingFeeIsPay = shippingFeeIsPay;
        Surcharge = surcharge;
        IsNoLogo = isNoLogo;
        AfterSalesServiceId = afterSalesServiceId ?? string.Empty;
        IsAssess = isAssess;
        Comments = comments ?? string.Empty;
        StartDate = startDate;
        EndDate = endDate;
        IsRed = isRed;
        IsFree = isFree;
        IsRepay = isRepay;
        RepayDate = repayDate;
        FRepayDate = fRepayDate;
        DelayDate = delayDate;
        DelayReason = delayReason ?? string.Empty;
        Feedback = feedback ?? string.Empty;
        Scontent = scontent ?? string.Empty;
        WarehousePickerId = warehousePickerId;
        WarehouseTechId = warehouseTechId;
        WarehouseReviewerId = warehouseReviewerId;
        WarehouseStatus = warehouseStatus;
        UpdatedAt = DateTimeOffset.UtcNow;
        Items.Clear();
        foreach (var item in items)
            Items.Add(new OrderItem(item));
    }

    /// <summary>
    /// 追加一条订单备注
    /// </summary>
    public void AddRemark(string content, UserId userId, int typeId)
    {
        Remarks.Add(new OrderRemark(content, userId, typeId));
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 修改订单备注内容（可选：限制只修改特定 TypeId）
    /// </summary>
    public void ChangeRemarkContent(OrderRemarkId remarkId, int expectedTypeId, string content)
    {
        var remark = Remarks.FirstOrDefault(r => r.Id == remarkId);
        if (remark == null)
            throw new KnownException("未找到订单备注", ErrorCodes.OrderRemarkNotFound);
        if (remark.TypeId != expectedTypeId)
            throw new KnownException("订单备注类型不匹配", ErrorCodes.OrderRemarkTypeMismatch);

        remark.ChangeContent(content);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 删除订单备注（可选：限制只删除特定 TypeId）
    /// </summary>
    public void RemoveRemark(OrderRemarkId remarkId, int expectedTypeId)
    {
        var remark = Remarks.FirstOrDefault(r => r.Id == remarkId);
        if (remark == null)
            throw new KnownException("未找到订单备注", ErrorCodes.OrderRemarkNotFound);
        if (remark.TypeId != expectedTypeId)
            throw new KnownException("订单备注类型不匹配", ErrorCodes.OrderRemarkTypeMismatch);

        Remarks.Remove(remark);
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 同步按分类的合同优惠行（全量替换）。完成后发布 <see cref="OrderUpdatedDomainEvent"/>，供应用层同步审批工作流变量等副作用。
    /// </summary>
    public void SyncOrderCategories(IReadOnlyCollection<(ProductCategoryId CategoryId, string CategoryName, decimal DiscountPoints, string Remark)> lines)
    {
        var emptyCategoryId = new ProductCategoryId(Guid.Empty);
        Categories.Clear();
        if (lines != null && lines.Count > 0)
        {
            foreach (var line in lines)
            {
                if (line.CategoryId == emptyCategoryId)
                    continue;
                Categories.Add(new OrderCategory(
                    Id,
                    line.CategoryId,
                    line.CategoryName ?? string.Empty,
                    line.DiscountPoints,
                    line.Remark ?? string.Empty));
            }
        }

        AddDomainEvent(new OrderUpdatedDomainEvent(this));
    }

    /// <summary>
    /// 软删除
    /// </summary>
    public void SoftDelete()
    {
        IsDeleted = true;
        DeletedAt = new DeletedTime(DateTimeOffset.UtcNow);
        AddDomainEvent(new OrderDeletedDomainEvent(this));
    }


    /// <summary>
    /// 请求启动订单审批工作流（仅发布领域事件；实际启流与回写实例由领域事件处理器完成）
    /// </summary>
    public void RequestOrderApprovalWorkflowStart(string remark)
    {
        if (Status != OrderStatus.Draft && Status != OrderStatus.Rejected)
        {
            throw new KnownException("只有草稿或已驳回的订单可以提交审批", ErrorCodes.OrderCannotSubmitForApproval);
        }

        AddDomainEvent(new OrderSubmitRequestedDomainEvent(this, remark ?? string.Empty));
    }

    /// <summary>
    /// 确认审批工作流已启动：关联工作流实例并将订单置为审核中（须在工作流实例已成功创建后、由应用层传入实例 ID）
    /// </summary>
    public void ConfirmApprovalWorkflowStarted(WorkflowInstanceId workflowInstanceId)
    {
        if (Status != OrderStatus.Draft && Status != OrderStatus.Rejected)
        {
            throw new KnownException("只有草稿或已驳回的订单可以提交审批", ErrorCodes.OrderCannotSubmitForApproval);
        }

        WorkflowInstanceId = workflowInstanceId;
        Status = OrderStatus.PendingAudit;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 审批通过（工作流完成后由领域事件处理器调用）
    /// </summary>
    public void Approve()
    {
        if (Status != OrderStatus.PendingAudit)
        {
            throw new KnownException("只有审核中的订单可以审批通过", ErrorCodes.OrderNotPendingAudit);
        }

        Status = OrderStatus.Ordered;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 审批驳回（工作流驳回后由领域事件处理器调用）
    /// </summary>
    public void Reject()
    {
        if (Status != OrderStatus.PendingAudit)
        {
            throw new KnownException("只有审核中的订单可以驳回", ErrorCodes.OrderNotPendingAudit);
        }

        Status = OrderStatus.Rejected;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 重新提交审批（仅已驳回的订单可操作）
    /// </summary>
    public void Resubmit(WorkflowInstanceId workflowInstanceId)
    {
        if (Status != OrderStatus.Rejected)
        {
            throw new KnownException("只有已驳回的订单可以重新提交", ErrorCodes.OrderNotRejected);
        }

        WorkflowInstanceId = workflowInstanceId;
        Status = OrderStatus.PendingAudit;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}

/// <summary>
/// 订单明细行创建/更新用数据（值对象），所有字段必填
/// </summary>
public record OrderItemData(
    ProductId ProductId,
    ProductCategoryId ProductCategoryId,
    ProductTypeId ProductTypeId,
    string ImagePath,
    string InstallNotes,
    string TrainingDuration,
    int PackingStatus,
    int ReviewStatus,
    string ProductName,
    string Model,
    string Number,
    int Qty,
    string Unit,
    decimal Price,
    decimal Amount,
    string Remark);
