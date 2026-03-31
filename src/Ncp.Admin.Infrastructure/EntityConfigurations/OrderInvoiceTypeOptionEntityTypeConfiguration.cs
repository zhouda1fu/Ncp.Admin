using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.OrderInvoiceTypeOptionAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

internal class OrderInvoiceTypeOptionEntityTypeConfiguration : IEntityTypeConfiguration<OrderInvoiceTypeOption>
{
    public void Configure(EntityTypeBuilder<OrderInvoiceTypeOption> builder)
    {
        builder.ToTable("order_invoice_type_option");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).ValueGeneratedOnAdd().HasComment("订单发票类型选项标识");
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100).HasComment("名称");
        builder.Property(x => x.TypeValue).IsRequired().HasComment("类型值");
        builder.Property(x => x.SortOrder).IsRequired().HasComment("排序");
    }
}
