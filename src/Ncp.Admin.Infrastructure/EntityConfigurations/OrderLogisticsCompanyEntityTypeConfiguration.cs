using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.OrderLogisticsCompanyAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

internal class OrderLogisticsCompanyEntityTypeConfiguration : IEntityTypeConfiguration<OrderLogisticsCompany>
{
    public void Configure(EntityTypeBuilder<OrderLogisticsCompany> builder)
    {
        builder.ToTable("order_logistics_company");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.TypeValue).IsRequired();
        builder.Property(x => x.Sort).IsRequired();
        builder.HasIndex(x => x.TypeValue);
        builder.HasIndex(x => x.Sort);
    }
}
