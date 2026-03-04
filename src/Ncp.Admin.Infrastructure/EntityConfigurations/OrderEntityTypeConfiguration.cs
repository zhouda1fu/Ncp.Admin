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
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.CustomerId).IsRequired();
        builder.Property(x => x.CustomerName).IsRequired().HasMaxLength(200);
        builder.Property(x => x.ProjectId).IsRequired();
        builder.Property(x => x.ContractId).IsRequired();
        builder.Property(x => x.OrderNumber).IsRequired().HasMaxLength(100);
        builder.Property(x => x.Type).IsRequired();
        builder.Property(x => x.Status).IsRequired();
        builder.Property(x => x.Amount).IsRequired().HasPrecision(18, 4);
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
        builder.Property(x => x.InstallationFee).IsRequired().HasPrecision(18, 4);
        builder.Property(x => x.EstimatedFreight).IsRequired().HasPrecision(18, 4);
        builder.Property(x => x.ContractFilesJson).IsRequired().HasMaxLength(8000);
        builder.Property(x => x.SelectedContractFileId).IsRequired().HasMaxLength(100);
        builder.Property(x => x.IsShipped).IsRequired();
        builder.Property(x => x.PaymentStatus).IsRequired().HasMaxLength(50);
        builder.Property(x => x.ContractNotCompanyTemplate).IsRequired();
        builder.Property(x => x.ContractDiscount).IsRequired().HasPrecision(18, 4);
        builder.Property(x => x.ContractAmount).IsRequired().HasPrecision(18, 4);
        builder.Property(x => x.ReceiverName).IsRequired().HasMaxLength(100);
        builder.Property(x => x.ReceiverPhone).IsRequired().HasMaxLength(50);
        builder.Property(x => x.ReceiverAddress).IsRequired().HasMaxLength(500);
        builder.Property(x => x.PayDate).IsRequired();
        builder.Property(x => x.DeliveryDate).IsRequired();
        builder.Property(x => x.IsDeleted).IsRequired();
        builder.Property(x => x.DeletedAt).IsRequired();
        builder.Property(x => x.CreatorId).IsRequired();
        builder.Property(x => x.CreatedAt).IsRequired();
        builder.Property(x => x.UpdatedAt).IsRequired();
        builder.HasIndex(x => x.OrderNumber);
        builder.HasIndex(x => x.CustomerId);
        builder.HasIndex(x => x.ContractId);
        builder.HasIndex(x => x.ProjectId);
        builder.HasIndex(x => x.OwnerId);
        builder.HasIndex(x => x.DeptId);
        builder.HasIndex(x => x.CreatedAt);
        builder.HasIndex(x => x.IsDeleted);

        builder.HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(o => o.Items).AutoInclude();
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
