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
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.Code).IsRequired().HasMaxLength(50);
        builder.Property(x => x.Title).IsRequired().HasMaxLength(200);
        builder.Property(x => x.PartyA).IsRequired().HasMaxLength(200);
        builder.Property(x => x.PartyB).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Amount).HasPrecision(18, 2);
        builder.Property(x => x.StartDate).IsRequired();
        builder.Property(x => x.EndDate).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.FileStorageKey).IsRequired().HasMaxLength(500);
        builder.Property(x => x.CreatorId).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdateTime);
        builder.Property(x => x.OrderId).IsRequired();
        builder.Property(x => x.CustomerId).IsRequired();
        builder.Property(x => x.ContractType).IsRequired();
        builder.Property(x => x.ContractTypeName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.IncomeExpenseType).IsRequired();
        builder.Property(x => x.IncomeExpenseTypeName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.SignDate).IsRequired();
        builder.Property(x => x.DepartmentId).IsRequired();
        builder.Property(x => x.BusinessManager).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ResponsibleProject).IsRequired().HasMaxLength(200);
        builder.Property(x => x.InputCustomer).IsRequired().HasMaxLength(200);
        builder.Property(x => x.NextPaymentReminder).IsRequired();
        builder.Property(x => x.ContractExpiryReminder).IsRequired();
        builder.Property(x => x.SingleDoubleSeal).IsRequired();
        builder.Property(x => x.InvoicingInformation).IsRequired().HasMaxLength(500);
        builder.Property(x => x.PaymentStatus).IsRequired();
        builder.Property(x => x.WarrantyPeriod).IsRequired().HasMaxLength(100);
        builder.Property(x => x.IsInstallmentPayment).IsRequired();
        builder.Property(x => x.AccumulatedAmount).IsRequired().HasPrecision(18, 2);
        builder.Property(x => x.Note).IsRequired().HasMaxLength(2000);
        builder.Property(x => x.Description).IsRequired().HasMaxLength(8000);
        builder.Property(x => x.ApprovedBy).IsRequired();
        builder.Property(x => x.ApprovedAt).IsRequired();
        builder.Property(x => x.IsDeleted).IsRequired();
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
