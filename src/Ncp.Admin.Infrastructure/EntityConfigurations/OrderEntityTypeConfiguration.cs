using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

internal class OrderEntityTypeConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("order");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator().HasComment("订单标识");
        builder.Property(x => x.CustomerId).IsRequired().HasComment("客户ID");
        builder.Property(x => x.CustomerName).IsRequired().HasMaxLength(200).HasComment("客户名称");
        builder.Property(x => x.ProjectId).IsRequired().HasComment("项目ID");
        builder.Property(x => x.OrderNumber).IsRequired().HasMaxLength(100).HasComment("订单编号");
        builder.Property(x => x.Type).IsRequired().HasComment("订单类型");
        builder.Property(x => x.Status).IsRequired().HasComment("订单状态");
        builder.Property(x => x.Amount).IsRequired().HasPrecision(18, 4).HasComment("金额");
        builder.Property(x => x.Remark).IsRequired().HasMaxLength(500);
        builder.Property(x => x.OwnerId).IsRequired();
        builder.Property(x => x.OwnerName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.DeptId).IsRequired();
        builder.Property(x => x.DeptName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ProjectContactName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ProjectContactPhone).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Warranty).IsRequired().HasMaxLength(50);
        builder.Property(x => x.ContractSigningCompany).IsRequired().HasMaxLength(200);
        builder.Property(x => x.ContractTrustee).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NeedInvoice).IsRequired();
        builder.Property(x => x.InvoiceTypeId).IsRequired().HasComment("发票类型ID");
        builder.Property(x => x.InstallationFee).IsRequired().HasPrecision(18, 4);
        builder.Property(x => x.EstimatedFreight).IsRequired().HasPrecision(18, 4);
        builder.Property(x => x.ContractFilesJson).IsRequired().HasMaxLength(8000);
        builder.Property(x => x.StockFilesJson).IsRequired().HasMaxLength(8000);
        builder.Property(x => x.SelectedContractFileId).IsRequired().HasComment("选择合同");
        builder.Property(x => x.IsShipped).IsRequired();
        builder.Property(x => x.PaymentStatus).IsRequired().HasComment("到款状态");
        builder.Property(x => x.ContractNotCompanyTemplate).IsRequired();
        builder.Property(x => x.ContractAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(x => x.ReceiverName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ReceiverPhone).IsRequired().HasMaxLength(50);
        builder.Property(x => x.ReceiverAddress).IsRequired().HasMaxLength(500);
        builder.Property(x => x.PayDate).IsRequired();
        builder.Property(x => x.DeliveryDate).IsRequired();
        builder.Property(x => x.OrderLogisticsCompanyId).IsRequired().HasComment("物流公司ID");
        builder.Property(x => x.OrderLogisticsMethodId).IsRequired().HasComment("物流方式ID");
        builder.Property(x => x.LogisticsPaymentMethodId).IsRequired().HasComment("物流费用支付方式");
        builder.Property(x => x.WaybillNumber).IsRequired().HasMaxLength(100).HasComment("运单编号");
        builder.Property(x => x.ShippingFee).IsRequired().HasPrecision(18, 4).HasComment("运费");
        builder.Property(x => x.ShippingFeeIsPay).IsRequired().HasComment("是否付运费");
        builder.Property(x => x.Surcharge).IsRequired().HasPrecision(18, 4).HasComment("附加费");
        builder.Property(x => x.WarehouseStatus).IsRequired().HasComment("仓库状态");
        builder.Property(x => x.WarehousePickerId).IsRequired().HasComment("配货人用户ID");
        builder.Property(x => x.WarehouseTechId).IsRequired().HasComment("仓库技术用户ID");
        builder.Property(x => x.WarehouseReviewerId).IsRequired().HasComment("复核人用户ID");
        builder.Property(x => x.IsDeleted).IsRequired().HasComment("是否软删");
        builder.Property(x => x.DeletedAt).IsRequired().HasComment("删除时间");
        builder.Property(x => x.CreatorId).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
        builder.Property(x => x.WorkflowInstanceId).IsRequired().HasComment("关联工作流实例ID（未关联为 Guid.Empty）");
        builder.HasIndex(x => x.OrderNumber);
        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.ProjectId);
        builder.HasIndex(x => x.OwnerId);
        builder.HasIndex(x => x.DeptId);
        builder.HasIndex(x => x.OrderLogisticsCompanyId);
        builder.HasIndex(x => x.OrderLogisticsMethodId);
        builder.HasIndex(x => x.WaybillNumber);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.IsDeleted);
        builder.HasIndex(x => x.WorkflowInstanceId);
        builder.HasQueryFilter(o => !o.IsDeleted);

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Categories)
            .WithOne()
            .HasForeignKey(c => c.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Remarks)
            .WithOne()
            .HasForeignKey(r => r.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

internal class OrderItemEntityTypeConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_item");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.OrderId).IsRequired();
        builder.Property(x => x.ProductId).IsRequired();
        builder.Property(x => x.ProductCategoryId).IsRequired();
        builder.Property(x => x.ProductTypeId).IsRequired();
        builder.Property(x => x.ImagePath).IsRequired().HasMaxLength(500);
        builder.Property(x => x.InstallNotes).IsRequired().HasMaxLength(500);
        builder.Property(x => x.TrainingDuration).IsRequired().HasMaxLength(100);
        builder.Property(x => x.PackingStatus).IsRequired();
        builder.Property(x => x.ReviewStatus).IsRequired();
        builder.Property(x => x.ProductName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Model).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Number).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Qty).IsRequired();
        builder.Property(x => x.Unit).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Price).IsRequired().HasPrecision(18, 4);
        builder.Property(x => x.Amount).IsRequired().HasPrecision(18, 4);
        builder.Property(x => x.Remark).IsRequired().HasMaxLength(500);
        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => x.ProductId);
    }
}

internal class OrderRemarkEntityTypeConfiguration : IEntityTypeConfiguration<OrderRemark>
{
    public void Configure(EntityTypeBuilder<OrderRemark> builder)
    {
        builder.ToTable("order_remark");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator().HasComment("订单备注标识");
        builder.Property(x => x.OrderId).IsRequired().HasComment("订单ID");
        builder.Property(x => x.AddedAt).IsRequired().HasComment("添加时间");
        builder.Property(x => x.UserId).IsRequired().HasComment("用户ID");
        builder.Property(x => x.TypeId).IsRequired().HasComment("类型ID");
        builder.Property(x => x.Content).IsRequired().HasMaxLength(2000).HasComment("说明内容");
        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => x.AddedAt);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => x.TypeId);
    }
}
