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
        builder.Property(x => x.Id).UseGuidVersion7ValueGenerator();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.Property(x => x.SortOrder).IsRequired();
        builder.HasIndex(x => x.SortOrder);
    }
}
