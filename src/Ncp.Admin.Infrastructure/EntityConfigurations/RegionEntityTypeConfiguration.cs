using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Ncp.Admin.Domain.AggregatesModel.RegionAggregate;

namespace Ncp.Admin.Infrastructure.EntityConfigurations;

internal class RegionEntityTypeConfiguration : IEntityTypeConfiguration<Region>
{
    public void Configure(EntityTypeBuilder<Region> builder)
    {
        builder.ToTable("region");
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Id).ValueGeneratedNever();
        builder.Property(r => r.Name).IsRequired().HasMaxLength(200);
        builder.Property(r => r.ParentId).IsRequired();
        builder.Property(r => r.Level).IsRequired();
        builder.Property(r => r.SortOrder).IsRequired();
        builder.HasIndex(r => r.ParentId);
        builder.HasIndex(r => r.Level);
    }
}
