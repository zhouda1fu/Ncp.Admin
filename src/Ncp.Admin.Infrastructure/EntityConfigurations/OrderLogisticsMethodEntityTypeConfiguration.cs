using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.OrderAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

internal class OrderLogisticsMethodEntityTypeConfiguration : IEntityTypeConfiguration<OrderLogisticsMethod>
{
    public void Configure(EntityTypeBuilder<OrderLogisticsMethod> builder)
    {
        builder.ToTable("order_logistics_method");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.TypeValue).IsRequired();
        builder.Property(x => x.Sort).IsRequired();
        builder.HasIndex(x => x.TypeValue);
        builder.HasIndex(x => x.Sort);
    }
}
