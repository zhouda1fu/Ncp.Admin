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
        builder.Property(r => r.Id).ValueGeneratedNever().HasComment("区域标识");
        builder.Property(r => r.Name).IsRequired().HasMaxLength(200).HasComment("区域名称");
        builder.Property(r => r.ParentId).IsRequired().HasComment("父级区域ID");
        builder.Property(r => r.Level).IsRequired().HasComment("层级");
        builder.Property(r => r.SortOrder).IsRequired().HasComment("排序");
        builder.HasIndex(r => r.ParentId);
        builder.HasIndex(r => r.Level);
    }
}
