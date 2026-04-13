using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.ContractAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

/// <summary>
/// 合同实体 EF Core 映射配置，表名 contract
/// </summary>
internal class ContractEntityTypeConfiguration : IEntityTypeConfiguration<Contract>
{
    /// <inheritdoc />
    public void Configure(EntityTypeBuilder<Contract> builder)
    {
        builder.ToTable("contract");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id)
            .UseGuidVersion7ValueGenerator()
            .HasComment("合同标识");
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50).HasComment("合同编号");
        builder.Property(x => x.Title).IsRequired().HasMaxLength(200).HasComment("合同标题");
        builder.Property(x => x.PartyA).IsRequired().HasMaxLength(200).HasComment("甲方");
        builder.Property(x => x.PartyB).IsRequired().HasMaxLength(200).HasComment("乙方");
        builder.Property(x => x.Amount).HasPrecision(18, 2).HasComment("合同金额");
        builder.Property(x => x.StartDate).IsRequired().HasComment("开始日期");
        builder.Property(x => x.EndDate).IsRequired().HasComment("结束日期（到期日）");
        builder.Property(x => x.Status).IsRequired().HasComment("合同状态");
        builder.Property(x => x.FileStorageKey).IsRequired().HasMaxLength(500).HasComment("附件存储 Key");
        builder.Property(x => x.CreatorId).IsRequired().HasComment("创建人用户ID");
        builder.Property(x => x.CreatedAt).IsRequired().HasComment("创建时间");
        builder.Property(x => x.UpdateTime).HasComment("更新时间");
        builder.Property(x => x.OrderId).IsRequired().HasComment("关联订单ID");
        builder.Property(x => x.CustomerId).IsRequired().HasComment("关联客户ID");
        builder.Property(x => x.ContractType).IsRequired().HasComment("合同类型（TypeValue）");
        builder.Property(x => x.ContractTypeName).IsRequired().HasMaxLength(200).HasComment("合同类型名称");
        builder.Property(x => x.IncomeExpenseType).IsRequired().HasComment("收支类型（TypeValue）");
        builder.Property(x => x.IncomeExpenseTypeName).IsRequired().HasMaxLength(200).HasComment("收支类型名称");
        builder.Property(x => x.SignDate).IsRequired().HasComment("签约日期");
        builder.Property(x => x.DepartmentId).IsRequired().HasComment("部门ID");
        builder.Property(x => x.BusinessManager).IsRequired().HasMaxLength(100).HasComment("业务经理");
        builder.Property(x => x.ResponsibleProject).IsRequired().HasMaxLength(200).HasComment("负责项目");
        builder.Property(x => x.InputCustomer).IsRequired().HasMaxLength(200).HasComment("录入客户（名称或标识）");
        builder.Property(x => x.NextPaymentReminder).IsRequired().HasComment("下次收付款报警");
        builder.Property(x => x.ContractExpiryReminder).IsRequired().HasComment("合同过期报警");
        builder.Property(x => x.SingleDoubleSeal).IsRequired().HasComment("单双章（0=单章 1=双章）");
        builder.Property(x => x.InvoicingInformation).IsRequired().HasMaxLength(500).HasComment("开票信息");
        builder.Property(x => x.PaymentStatus).IsRequired().HasComment("到款情况（TypeValue）");
        builder.Property(x => x.WarrantyPeriod).IsRequired().HasMaxLength(100).HasComment("质保期");
        builder.Property(x => x.IsInstallmentPayment).IsRequired().HasComment("是否分期");
        builder.Property(x => x.AccumulatedAmount).IsRequired().HasPrecision(18, 2).HasComment("累计金额");
        builder.Property(x => x.Note).IsRequired().HasMaxLength(2000).HasComment("备注");
        builder.Property(x => x.Description).IsRequired().HasMaxLength(8000).HasComment("合同内容/描述");
        builder.Property(x => x.ApprovedBy).IsRequired().HasComment("审批人");
        builder.Property(x => x.ApprovedAt).IsRequired().HasComment("审批时间");
        builder.Property(x => x.IsDeleted).IsRequired().HasComment("是否软删");
        builder.HasIndex(x => x.Code);
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.EndDate);
        builder.HasIndex(x => x.OrderId);
        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.SignDate);
        builder.HasIndex(x => x.IsDeleted);

        builder.HasMany(c => c.Invoices)
            .WithOne()
            .HasForeignKey(i => i.ContractId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(c => c.Invoices).AutoInclude();
    }
}

internal class ContractInvoiceEntityTypeConfiguration : IEntityTypeConfiguration<ContractInvoice>
{
    public void Configure(EntityTypeBuilder<ContractInvoice> builder)
    {
        builder.ToTable("contract_invoice");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.ContractId).IsRequired();
        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.InvoiceNumber).IsRequired().HasMaxLength(100);
        builder.Property(x => x.TaxRate).IsRequired().HasPrecision(18, 4);
        builder.Property(x => x.AmountExclTax).IsRequired().HasPrecision(18, 2);
        builder.Property(x => x.Source).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.TaxAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(x => x.InvoicedAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(x => x.Handler).IsRequired().HasMaxLength(100);
        builder.Property(x => x.BillingDate).IsRequired();
        builder.Property(x => x.Remarks).IsRequired().HasMaxLength(2000);
        builder.Property(x => x.AttachmentStorageKey).IsRequired().HasMaxLength(500);
        builder.HasIndex(x => x.ContractId);
    }
}
