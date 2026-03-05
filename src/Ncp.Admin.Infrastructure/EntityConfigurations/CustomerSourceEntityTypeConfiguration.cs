using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.CustomerSourceAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

internal class CustomerSourceEntityTypeConfiguration : IEntityTypeConfiguration<CustomerSource>
{
    public void Configure(EntityTypeBuilder<CustomerSource> builder)
    {
        builder.ToTable("customer_source");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator().HasComment("客户来源标识");
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100).HasComment("名称");
        builder.Property(x => x.SortOrder).IsRequired().HasComment("排序");
        builder.HasIndex(x => x.SortOrder);
    }
}
